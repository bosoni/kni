// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    internal class CurveReader : ContentTypeReader<Curve>
    {
        protected internal override Curve Read(ContentReader input, Curve existingInstance)
        {
            Curve curve = existingInstance;
            if (curve == null)
            {
                curve = new Curve();
            }         
            
            curve.PreLoop = (CurveLoopType)input.ReadInt32();
            curve.PostLoop = (CurveLoopType)input.ReadInt32();
            int count = input.ReadInt32();
            
            for (int i = 0; i < count; i++)
            {
                float position = input.ReadSingle();
                float value = input.ReadSingle();
                float tangentIn = input.ReadSingle();
                float tangentOut = input.ReadSingle();
                CurveContinuity continuity = (CurveContinuity)input.ReadInt32();
                curve.Keys.Add(new CurveKey(position, value, tangentIn, tangentOut, continuity));
            }		
            return curve;         
        }
    }
}

