using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Churras.Domain.Tests.Builders
{
    public class BbqBuilder
    {
        public string? _id;
        public BbqCart _bbqCart;
        public bool _isGonnaHappen;
        public BbqStatus? _bbqStatus;

        public BbqBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public BbqBuilder WithBbqCart(BbqCart bbqCart)
        {
            _bbqCart = bbqCart;
            return this;
        }

        public BbqBuilder WithStatus(BbqStatus bbqStatus)
        {
            _bbqStatus = bbqStatus;
            return this;
        }

        public Bbq Build()
        {
            return new Bbq
            {
                Id = _id ?? string.Empty,
                BbqCart = _bbqCart,
                Status = _bbqStatus ?? BbqStatus.New,
            };
        }
    }
}
