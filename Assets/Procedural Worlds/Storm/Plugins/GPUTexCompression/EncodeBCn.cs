using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class EncodeBCn
{
    public enum Format
    {
        None,
        BC1_XDK,
        BC3_XDK,
    }
    
    static readonly int s_Prop_Source = Shader.PropertyToID("_Source");
    static readonly int s_Prop_Target = Shader.PropertyToID("_Target");
    static readonly int s_Prop_Quality = Shader.PropertyToID("_Quality");
    static readonly int s_Prop_Lod = Shader.PropertyToID("_Lod");
    static readonly int s_Prop_EncodeBCn_Temp = Shader.PropertyToID("_EncodeBCn_Temp");

    private ComputeShader m_EncodeShader;
    private int m_EncodeKernelBC1_XDK;
    private int m_EncodeKernelBC3_XDK;

    public EncodeBCn(ComputeShader encodeShader)
    {
        Assert.IsNotNull(encodeShader);
        m_EncodeShader = encodeShader;
        m_EncodeKernelBC1_XDK = m_EncodeShader.FindKernel("EncodeBC1_XDK");
        m_EncodeKernelBC3_XDK = m_EncodeShader.FindKernel("EncodeBC3_XDK");
    }

    public void Encode(CommandBuffer cmb,
        RenderTargetIdentifier source, int sourceWidth, int sourceHeight,
        RenderTargetIdentifier target,
        Format format, float quality)
    {
        if (format == Format.None)
        {
            cmb.CopyTexture(source, 0, target, 0);
            return;
        }
        int tempWidth = (sourceWidth + 3) / 4;
        int tempHeight = (sourceHeight + 3) / 4;
        var desc = new RenderTextureDescriptor
        {
            width = tempWidth,
            height = tempHeight,
            dimension = TextureDimension.Tex2D,
            enableRandomWrite = true,
            msaaSamples = 1,
            useMipMap = false,
            mipCount = 0,
            autoGenerateMips = false
        };

        int kernelIndex = format switch
        {
            Format.BC1_XDK => m_EncodeKernelBC1_XDK,
            Format.BC3_XDK => m_EncodeKernelBC3_XDK,
            _ => -1
        };
        desc.graphicsFormat = format switch
        {
            Format.BC1_XDK => GraphicsFormat.R32G32_SInt,
            _ => GraphicsFormat.R32G32B32A32_SInt
        };
        m_EncodeShader.GetKernelThreadGroupSizes(kernelIndex, out var sizeX, out var sizeY, out var sizeZ);
        cmb.GetTemporaryRT(s_Prop_EncodeBCn_Temp, desc);
        cmb.SetComputeFloatParam(m_EncodeShader, s_Prop_Quality, quality);
        cmb.SetComputeTextureParam(m_EncodeShader, kernelIndex, s_Prop_Source, source, 0);
        cmb.SetComputeTextureParam(m_EncodeShader, kernelIndex, s_Prop_Target, s_Prop_EncodeBCn_Temp, 0);
        cmb.SetComputeIntParam(m_EncodeShader, s_Prop_Lod, 0);
        cmb.DispatchCompute(m_EncodeShader, kernelIndex, (int)((tempWidth + sizeX - 1) / sizeX), (int)((tempHeight + sizeY - 1) / sizeY), 1);
        cmb.CopyTexture(s_Prop_EncodeBCn_Temp,0, 0, target, 0,0);

        cmb.ReleaseTemporaryRT(s_Prop_EncodeBCn_Temp);
    }


    public void EncodeWithMips(CommandBuffer cmb,
        RenderTargetIdentifier source, int sourceWidth, int sourceHeight,
        RenderTargetIdentifier target,
        Format format, float quality)
    {
        if (format == Format.None)
        {
            cmb.CopyTexture(source, 0, target, 0);
            return;
        }
        int tempWidth = (sourceWidth + 3) / 4;
        int tempHeight = (sourceHeight + 3) / 4;
        var desc = new RenderTextureDescriptor
        {
            width = tempWidth,
            height = tempHeight,
            dimension = TextureDimension.Tex2D,
            enableRandomWrite = true,
            msaaSamples = 1,
            useMipMap = true,
            mipCount = 2,
            autoGenerateMips = true
        };

        int kernelIndex = format switch
        {
            Format.BC1_XDK => m_EncodeKernelBC1_XDK,
            Format.BC3_XDK => m_EncodeKernelBC3_XDK,
            _ => -1
        };
        desc.graphicsFormat = format switch
        {
            Format.BC1_XDK => GraphicsFormat.R32G32_SInt,
            _ => GraphicsFormat.R32G32B32A32_SInt
        };

        m_EncodeShader.GetKernelThreadGroupSizes(kernelIndex, out var sizeX, out var sizeY, out var sizeZ);
        cmb.GetTemporaryRT(s_Prop_EncodeBCn_Temp, desc);
        cmb.SetComputeFloatParam(m_EncodeShader, s_Prop_Quality, quality);
        cmb.SetComputeTextureParam(m_EncodeShader, kernelIndex, s_Prop_Source, source, 0);
        cmb.SetComputeTextureParam(m_EncodeShader, kernelIndex, s_Prop_Target, s_Prop_EncodeBCn_Temp, 0);
        cmb.SetComputeIntParam(m_EncodeShader, s_Prop_Lod, 0);
        cmb.DispatchCompute(m_EncodeShader, kernelIndex, (int)((tempWidth + sizeX - 1) / sizeX), (int)((tempHeight + sizeY - 1) / sizeY), 1);
        
        //Mip 1
        cmb.SetComputeTextureParam(m_EncodeShader, kernelIndex, s_Prop_Source, source, 1);
        cmb.SetComputeTextureParam(m_EncodeShader, kernelIndex, s_Prop_Target, s_Prop_EncodeBCn_Temp, 1);
        cmb.SetComputeIntParam(m_EncodeShader, s_Prop_Lod, 1);
        cmb.DispatchCompute(m_EncodeShader, kernelIndex, 16, 16, 1);

        //Mip 0
        cmb.CopyTexture(s_Prop_EncodeBCn_Temp, 0, 0, target, 0, 0);

        //Mip 1
        cmb.CopyTexture(s_Prop_EncodeBCn_Temp, 0, 1, target, 0, 1);
        cmb.ReleaseTemporaryRT(s_Prop_EncodeBCn_Temp);
    }

}
