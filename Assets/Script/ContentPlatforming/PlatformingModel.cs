using ChracterDefine;
using UnityEngine;

public class PlatformingModel : MonoBehaviour
{
    [field:SerializeField] public float JumpForce { get; private set; } = 1;
    private UserCharacterData baseCharacterData;
    private int jumpCount = 2;
    private int curJumpCount = 0;

    public void SetupModel()
    {
        JumpForce = 8;
        ResetCurJumpCount();
    }

    public bool CanJump() => curJumpCount > 0;

    public void IncreaseCurJumpCount(int value)
    {
        curJumpCount += value;
    }

    public void ResetCurJumpCount()
    {
        curJumpCount = jumpCount;
    }
}
