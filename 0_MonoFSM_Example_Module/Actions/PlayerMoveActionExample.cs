using Cysharp.Threading.Tasks;
using MonoFSM.Core.Runtime.Action;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveActionExample : AbstractStateAction
{
    public Rigidbody rigidBody;

    private float _speed = 100f;

    public InputActionReference moveInput;

    [ReadOnly]
    public float direction;
    
    protected override void OnActionExecuteImplement()
    {
        direction = moveInput.action.ReadValue<Vector2>().x;
        
        Vector3 velocity = rigidBody.linearVelocity;
        velocity.x = direction * _speed;
        rigidBody.linearVelocity = velocity;

        if (Mathf.Abs(direction) > 0.1f)
        {
            rigidBody.transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
        }
    }
}
