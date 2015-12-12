﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes
{
    public class GuidDatum : Datum
    {
        private Guid value;

        public GuidDatum(Guid value)
        {
            this.value = value;
        }

        public Guid Value => value;

        public override DatumType DatumType => DatumType.Guid;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitGuid(this);
    }
}
