namespace Domain.Entities
{
    public class BbqCart
    {
        public BbqCart(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
        public double Veggies { get; set; } = default;
        public double Meat { get; set; } = default;

        public void IncreaseQuantitiesByIsVeg(bool isVeg)
        {
            if (isVeg)
            {
                Veggies += 600;
            }
            else
            {
                Veggies += 300;
                Meat += 300;
            }
        }

        public void DecreaseQuantitiesByIsVeg(bool isVeg)
        {
            if (isVeg)
            {
                Veggies -= 600;
            }
            else
            {
                Veggies -= 300;
                Meat -= 300;
            }
        }

        public override string ToString()
        {
            return $"There are {Meat / 1000}kg of Meat and {Veggies / 1000}kg of Veggies for the barbecue";
        }

        public object? TakeSnapshot()
        {
            return new
            {
                Id,
                Veggies = Veggies / 1000,
                Meat = Meat / 1000,
            };
        }
    }
}