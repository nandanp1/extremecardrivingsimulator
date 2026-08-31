namespace Unibill.Impl
{
	public class WritablePurchasable
	{
		public PurchasableItem item { get; private set; }

		public string Id
		{
			get
			{
				return item.Id;
			}
			set
			{
				item.Id = value;
			}
		}

		public PurchaseType PurchaseType
		{
			get
			{
				return item.PurchaseType;
			}
			set
			{
				item.PurchaseType = value;
			}
		}

		public string description
		{
			get
			{
				return item.description;
			}
			set
			{
				item.description = value;
			}
		}

		public string name
		{
			get
			{
				return item.name;
			}
			set
			{
				item.name = value;
			}
		}

		public WritablePurchasable(PurchasableItem item)
		{
			this.item = item;
		}
	}
}
