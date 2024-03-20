using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MathHelper
{
    const float APPROXMATE = 0.1f;

    public static Vector3 CardLerp(Vector2 startV, Vector2 targetV, float speed)
    {
        float startX = startV.x;
        float targetX = targetV.x;

        if (startX != targetX)
        {
            startX = Mathf.Lerp(startX, targetX, Time.deltaTime * speed);
            if (Mathf.Abs(startX - targetX) <= APPROXMATE)
                startX = targetX;    
        }


        float startY = startV.y;
        float targetY = targetV.y;

        if (startY != targetY)
        {
            startY = Mathf.Lerp(startY, targetY, Time.deltaTime * speed);
            if (Mathf.Abs(startY - targetY) <= APPROXMATE)
                startY = targetY;
        }

        return new Vector3(startX, startY, 0);

    }

    public static float CardRotateLerp(float start, float target, float speed)
    {
        float startR = start;
        if (start != target)
        {
            startR = Mathf.Lerp(start, target, Time.deltaTime * speed);
            if (Mathf.Abs(start - target) <= APPROXMATE)
                startR = target;
        }


        return startR;
    }
}
