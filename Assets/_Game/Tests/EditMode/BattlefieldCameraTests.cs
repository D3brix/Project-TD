using NUnit.Framework;
using ProjectTD.Levels;
using UnityEngine;

namespace ProjectTD.Tests
{
    public class BattlefieldCameraTests
    {
        [TestCase(16f / 9f)]
        [TestCase(4f / 3f)]
        [TestCase(21f / 9f)]
        [TestCase(16f / 10f)]
        public void TheWholeBattlefield_FitsBetweenTheHudBars_AtAnyAspect(float aspect)
        {
            var area = new Rect(-14f, -6.5f, 28f, 13f);
            const float top = 0.065f, bottom = 0.11f;

            float size = BattlefieldCamera.RequiredOrthographicSize(area.size, aspect, top, bottom);
            float cameraY = BattlefieldCamera.CameraY(area.center.y, size, top, bottom);

            float viewBottom = cameraY - size;
            float viewHeight = size * 2f;
            float bandBottom = viewBottom + viewHeight * bottom;
            float bandTop = viewBottom + viewHeight * (1f - top);
            float halfWidth = size * aspect;

            Assert.LessOrEqual(bandBottom, area.yMin + 1e-4f, "Not hidden under the bottom bar.");
            Assert.GreaterOrEqual(bandTop, area.yMax - 1e-4f, "Not hidden under the top bar.");
            Assert.GreaterOrEqual(halfWidth, area.width / 2f - 1e-4f, "Not cut off at the sides.");
        }
    }
}
