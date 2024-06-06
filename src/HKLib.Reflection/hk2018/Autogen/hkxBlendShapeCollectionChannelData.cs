// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxBlendShapeCollectionChannelData : HavokData<hkxBlendShapeCollectionChannel> 
{
    public hkxBlendShapeCollectionChannelData(HavokType type, hkxBlendShapeCollectionChannel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_blendShapes":
            case "blendShapes":
            {
                if (instance.m_blendShapes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (instance.m_vertData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_blendShapes":
            case "blendShapes":
            {
                if (value is not List<hkxBlendShapeCollectionChannel.BlendShape> castValue) return false;
                instance.m_blendShapes = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (value is not hkxVertexBuffer castValue) return false;
                instance.m_vertData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}