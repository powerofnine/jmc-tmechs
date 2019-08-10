using UnityEngine;

namespace TMechs.Player
{
    public class CameraObstructorController : MonoBehaviour
    {
        private static readonly int PLAYER_POSITION = Shader.PropertyToID("_PlayerPosition");
        
        private void LateUpdate()
        { 
            Shader.SetGlobalVector(PLAYER_POSITION, transform.position.Remove(Utility.Axis.Y));
        }
    }
}