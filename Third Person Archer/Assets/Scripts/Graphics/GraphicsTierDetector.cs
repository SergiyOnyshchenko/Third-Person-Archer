using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// First-launch guess of the quality level from the device: memory, cores, graphics API and GPU family.
/// The guess only has to be close: GraphicsRuntime steps it down if the device can't hold the frame rate.
/// </summary>
public static class GraphicsTierDetector
{
    private enum GpuClass
    {
        Unknown,
        Low,
        Medium,
        High
    }

    public static GraphicsTier Detect()
    {
        int memory = SystemInfo.systemMemorySize;

        if (!Application.isMobilePlatform)
            return GraphicsTier.High;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (memory >= 5500)
                return GraphicsTier.High;
            return memory >= 2900 ? GraphicsTier.Medium : GraphicsTier.Low;
        }

        GpuClass gpu = ClassifyGpu(SystemInfo.graphicsDeviceName);
        bool vulkan = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan;

        // 3 GB phones report ~2800 MB, 4 GB ones ~3600 MB.
        if (memory < 3000 || SystemInfo.processorCount <= 4 || gpu == GpuClass.Low)
            return GraphicsTier.Low;

        if (gpu == GpuClass.High && vulkan && memory >= 5500)
            return GraphicsTier.High;

        return GraphicsTier.Medium;
    }

    private static GpuClass ClassifyGpu(string name)
    {
        if (string.IsNullOrEmpty(name))
            return GpuClass.Unknown;

        // Qualcomm: Adreno 730+ (8 Gen 1 and newer) is high, 616+ and the 7xx mid-range is medium.
        Match adreno = Regex.Match(name, @"Adreno[^\d]*(\d{3})");
        if (adreno.Success)
        {
            int number = int.Parse(adreno.Groups[1].Value);
            int series = number / 100;
            int model = number % 100;

            if (series >= 7 && model >= 30)
                return GpuClass.High;
            if (series >= 7 || (series == 6 && model >= 16))
                return GpuClass.Medium;
            return GpuClass.Low;
        }

        // Arm: Immortalis and Mali-G7xx are high, G6xx/G5xx and the G76-G78 generation medium, the rest low.
        if (name.Contains("Immortalis"))
            return GpuClass.High;

        Match mali = Regex.Match(name, @"Mali-G(\d+)");
        if (mali.Success)
        {
            int number = int.Parse(mali.Groups[1].Value);

            if (number >= 100)
            {
                int series = number / 100;
                if (series >= 7)
                    return GpuClass.High;
                return series >= 5 ? GpuClass.Medium : GpuClass.Low;
            }

            return number >= 76 || number == 68 ? GpuClass.Medium : GpuClass.Low;
        }

        if (name.Contains("Mali") || name.Contains("PowerVR"))
            return GpuClass.Low;

        // Samsung's own GPUs (Exynos 2200 and newer).
        if (name.Contains("Xclipse"))
            return GpuClass.High;

        return GpuClass.Unknown;
    }
}
