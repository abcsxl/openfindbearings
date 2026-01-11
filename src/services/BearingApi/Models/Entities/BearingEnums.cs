namespace BearingApi.Models.Entities
{
    public enum BearingType
    {
        DeepGrooveBallBearing,      // 深沟球轴承
        AngularContactBallBearing,  // 角接触球轴承
        SelfAligningBallBearing,    // 调心球轴承
        CylindricalRollerBearing,   // 圆柱滚子轴承
        TaperedRollerBearing,       // 圆锥滚子轴承
        SphericalRollerBearing,     // 调心滚子轴承
        NeedleRollerBearing,        // 滚针轴承
        ThrustBallBearing,          // 推力球轴承
        ThrustRollerBearing,        // 推力滚子轴承
        PillowBlockBearing,         // 带座轴承
        FlangeBearing,              // 法兰轴承
        Other                       // 其他
    }

    public enum BearingCategory
    {
        Standard,                   // 标准轴承
        Precision,                  // 精密轴承
        HighTemperature,            // 高温轴承
        CorrosionResistant,         // 耐腐蚀轴承
        HighSpeed,                  // 高速轴承
        HeavyDuty,                  // 重载轴承
        Miniature,                  // 微型轴承
        Custom                      // 定制轴承
    }

    public enum BearingStatus
    {
        Pending,
        Draft,                      // 草稿
        Active,                     // 活跃
        Inactive,                   // 停用
        Archived,                   // 归档
        Deleted                     // 删除
    }

    public enum VerificationLevel
    {
        Pending,                    // 待审核
        Basic,                      // 基础验证
        Standard,                   // 标准验证
        Advanced,                   // 高级验证
        Certified                   // 认证通过
    }

    public enum BearingParameter
    {
        InnerDiameter,
        OuterDiameter,
        Width,
        Weight,
        DynamicLoadRating,
        StaticLoadRating,
        LimitingSpeed,
        Material,
        Brand,
        Standard
    }

    //public enum BearingType
    //{
    //    DeepGrooveBallBearing,
    //    AngularContactBallBearing,
    //    SelfAligningBallBearing,
    //    ThrustBallBearing,
    //    CylindricalRollerBearing,
    //    TaperedRollerBearing,
    //    SphericalRollerBearing,
    //    NeedleRollerBearing,
    //    ThrustRollerBearing,
    //    PillowBlockBearing,
    //    FlangedBearing,
    //    Other
    //}

    //public enum BearingCategory
    //{
    //    Standard,
    //    Precision,
    //    HeavyDuty,
    //    HighTemperature,
    //    CorrosionResistant,
    //    Ceramic,
    //    Plastic,
    //    Custom
    //}

    //public enum BearingStatus
    //{
    //    Draft,
    //    Pending,
    //    Active,
    //    Inactive,
    //    Rejected,
    //    Archived
    //}

    //public enum VerificationLevel
    //{
    //    None,
    //    Basic,
    //    Standard,
    //    Premium
    //}
}
