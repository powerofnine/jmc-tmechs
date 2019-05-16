using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebugConsole
{
    public class DebugConsole : MonoBehaviour
    {
        public const string INTRO_STRING = "Simple Debug Console v1.0";
        
        public static DebugConsole Instance { get; private set; }

        public event Action<bool> OnConsoleToggle;
        
        private readonly Dictionary<Type, IDebugTypeParser> parsers = new Dictionary<Type, IDebugTypeParser>();
        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();

        private GameObject consoleDisplay;
        private TextMeshProUGUI consoleText;
        private TMP_InputField consoleField;
        private ScrollRect consoleScroll;

        private void Update()
        {
            if (consoleDisplay.activeSelf && consoleField && EventSystem.current)
                consoleField.OnSelect(null);

            if (Input.GetKeyDown(KeyCode.BackQuote))
                ToggleConsole();

            if ((Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && consoleDisplay.activeSelf)
            {
                if (!consoleField)
                    return;
                Execute(consoleField.text);
                consoleField.text = "";
            }
        }

        public void ToggleConsole()
        {
            SetConsole(!consoleDisplay.activeSelf);
        }

        public void SetConsole(bool open)
        {
            consoleDisplay.SetActive(open);
            OnConsoleToggle?.Invoke(open);
        }

        public void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            AddMessage($"] {command}", Color.cyan);
            
            command = command.Trim();

            List<string> args = command.Split(' ').ToList();
            command = args[0];
            args.RemoveAt(0);

            // Collapse quoted arguments
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].StartsWith("\""))
                {
                    args[i] = args[i].Substring(1);
                    for (int j = i + 1; j < args.Count && !args[i].EndsWith("\"");)
                    {
                        args[i] += " " + args[j];
                        args.RemoveAt(j);
                    }

                    args[i] = args[i].Substring(0, args[i].Length - 1);
                }
            }
            
            if (!commands.ContainsKey(command))
            {
                AddMessage($"[CONSOLE] {command} is not a recognized command", Color.red);
                return;
            }

            commands[command].Execute(args.ToArray());
        }

        private void InitializeConsole(IEnumerable<CommandDiscovery> discoveries)
        {
            Instance = this;
            
            // Setup debug console
            consoleDisplay = Instantiate(Resources.Load<GameObject>("DebugConsole/Console"), transform);
            consoleDisplay.SetActive(false);

            consoleText = consoleDisplay.GetComponentsInChildren<TextMeshProUGUI>(true).SingleOrDefault(x => "ConsoleText".Equals(x.name));
            consoleField = consoleDisplay.GetComponentInChildren<TMP_InputField>(true);
            consoleScroll = consoleDisplay.GetComponentInChildren<ScrollRect>(true);
            consoleDisplay.GetComponentsInChildren<Button>(true).SingleOrDefault(x => "Close".Equals(x.name))?.onClick.AddListener(() => SetConsole(false));
            
            Clear();
            Application.logMessageReceivedThreaded += HandleDebugLog;

            // Setup argv parsers
            Type[] discoveredParsers = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from t in assembly.GetTypes()
                    where t.GetInterfaces().Contains(typeof(IDebugTypeParser)) && !t.IsAbstract
                    select t).ToArray();
            foreach (Type parser in discoveredParsers)
            {
                IDebugTypeParser instance = (IDebugTypeParser) parser.GetConstructor(new Type[] { })?.Invoke(new object[] { });

                if (instance == null)
                {
                    Debug.LogWarning($"[CONSOLE] Could not find a valid constructor for parser {parser.FullName}, skipping...");
                    continue;
                }

                if (parsers.ContainsKey(instance.GetTarget()))
                {
                    Debug.LogWarning($"[CONSOLE] A parser for {instance.GetTarget()} is already defined when trying to add {parser.FullName}, skipping...");
                    continue;
                }

                parsers.Add(instance.GetTarget(), instance);
            }

            // Setup commands
            foreach (CommandDiscovery discovery in discoveries)
            {
                if (!commands.ContainsKey(discovery.attribute.command))
                    commands.Add(discovery.attribute.command, new Command(discovery.attribute.command));

                commands[discovery.attribute.command].Add(discovery);
            }
        }

        private void HandleDebugLog(string message, string trace, LogType type)
        {
            Color c = default;

            switch (type)
            {
                case LogType.Log:
                    c = Color.white;
                    break;
                case LogType.Warning:
                    c = Color.yellow;
                    break;
                case LogType.Assert:
                    c = Color.green;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    c = Color.red;
                    break;
            }

            AddMessage(message, c);
        }

        public void AddMessage(string message, Color c)
        {
            if (consoleText)
                consoleText.text += $"<#{ColorUtility.ToHtmlStringRGB(c)}>{message.Trim('\t', '\n', ' ')}</color>\n";
            if (consoleScroll)
                consoleScroll.verticalNormalizedPosition = 0F;
        }

        public void Clear()
        {
            if (consoleText)
                consoleText.text = INTRO_STRING;
        }

        private Type[] ExpandArguments(IEnumerable<Type> args)
        {
            List<Type> expanded = new List<Type>();

            foreach (Type arg in args)
            {
                if (parsers.ContainsKey(arg))
                {
                    parsers[arg].AppendArguments(expanded);
                    continue;
                }

                if (arg.IsEnum)
                {
                    expanded.Add(typeof(string));
                    continue;
                }

                if (typeof(IConvertible).IsAssignableFrom(arg) || typeof(string).IsAssignableFrom(arg))
                {
                    expanded.Add(arg);
                    continue;
                }

                Debug.LogError($"[CONSOLE] Could not find a parser for type {arg.FullName}");
            }

            return expanded.ToArray();
        }

        private object[] ParseArguments(string[] args, IEnumerable<Type> argList, Type[] expandedArgList)
        {
            Queue<object> argQueue = new Queue<object>();

            for (int i = 0; i < args.Length; i++)
            {
                if (!expandedArgList[i].IsPrimitive && !typeof(string).IsAssignableFrom(expandedArgList[i]))
                {
                    Debug.LogError($"[CONSOLE] Expanded argument of type {expandedArgList[i].FullName} is not a primitive or string");
                    return null;
                }

                if (typeof(string).IsAssignableFrom(expandedArgList[i]))
                {
                    argQueue.Enqueue(args[i]);
                    continue;
                }

                if (typeof(IConvertible).IsAssignableFrom(expandedArgList[i]))
                {
                    try
                    {
                        argQueue.Enqueue(Convert.ChangeType(args[i], expandedArgList[i]));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[CONSOLE] argument {args[i]} cannot be parsed into type {expandedArgList[i].FullName}");
                        Debug.LogError(e.Message);
                        return null;
                    }
                }
            }

            List<object> finalArgs = new List<object>();
            foreach (Type arg in argList)
            {
                if (parsers.ContainsKey(arg))
                {
                    finalArgs.Add(parsers[arg].GetValue(argQueue));
                    continue;
                }

                if (arg.IsEnum)
                {
                    List<string> names = Enum.GetNames(arg).ToList();
                    string val = (string) argQueue.Dequeue();
                    
                    if (!names.Contains(val))
                    {
                        Debug.LogError($"[CONSOLE] Illegal value given {val}, legal values are: {string.Join(", ", names)}");
                        return null;
                    }
                    
                    finalArgs.Add(Enum.Parse(arg, val));
                    continue;
                }
                
                if (typeof(IConvertible).IsAssignableFrom(arg) || typeof(string).IsAssignableFrom(arg))
                {
                    finalArgs.Add(argQueue.Dequeue());
                    continue;
                }

                Debug.LogError($"[CONSOLE] cannot parse argument of type {arg.FullName}");
                return null;
            }

            return finalArgs.ToArray();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            CommandDiscovery[] commands = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from t in assembly.GetTypes()
                    from f in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    let attrib = f.GetCustomAttribute<DebugConsoleCommand>(false)
                    where attrib != null
                    select new CommandDiscovery {method = f, attribute = attrib}).ToArray();

            GameObject go = new GameObject("Debug Console");
            DontDestroyOnLoad(go);
            go.AddComponent<DebugConsole>().InitializeConsole(commands);
        }

        private class Command
        {
            private string command;
            private readonly Dictionary<int, SubCommand> subs = new Dictionary<int, SubCommand>();

            public Command(string command)
            {
                this.command = command;
            }

            public void Add(CommandDiscovery discovery)
            {
                SubCommand sub = new SubCommand(discovery.method, discovery.method.GetParameters().Select(x => x.ParameterType).ToArray());

                int argc = sub.GetArgc();
                if (subs.ContainsKey(argc))
                {
                    Debug.LogError($"[CONSOLE] Command {discovery.attribute.command} has multiple candidates with {argc} arguments");
                    return;
                }

                subs.Add(argc, sub);
            }

            public void Execute(string[] args)
            {
                if (!subs.ContainsKey(args.Length))
                {
                    Instance.AddMessage($"[CONSOLE] {command} does not accept {args.Length} arguments", Color.red);
                    return;
                }

                subs[args.Length].Execute(args);
            }

            private struct SubCommand
            {
                private readonly MethodInfo method;

                private readonly Type[] arguments;
                private readonly Type[] expandedArguments;

                public SubCommand(MethodInfo method, Type[] arguments)
                {
                    this.method = method;

                    this.arguments = arguments;
                    expandedArguments = Instance.ExpandArguments(arguments);
                }

                public void Execute(string[] args)
                {
                    object[] processed = Instance.ParseArguments(args, arguments, expandedArguments);

                    if (processed == null)
                        return;
                    
                    try
                    {
                        method.Invoke(null, processed);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[CONSOLE]: " + e.Message);
                        Debug.LogError(e.StackTrace);
                    }
                }

                [Pure]
                public int GetArgc()
                {
                    return expandedArguments.Length;
                }
            }
        }

        private struct CommandDiscovery
        {
            public MethodInfo method;
            public DebugConsoleCommand attribute;
        }
    }
}