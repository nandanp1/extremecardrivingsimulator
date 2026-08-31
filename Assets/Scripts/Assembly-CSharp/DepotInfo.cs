using UnityEngine;

public class DepotInfo : MonoBehaviour
{
	private string Location;

	private string key;

	public GameObject coche1;

	public GameObject coche2;

	public GameObject coche3;

	public GameObject truck1;

	private int split;

	private int lenght;

	private string bucket;

	private string file;

	private krptAES krpt = new krptAES();

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.transform.gameObject);
	}

	private void Start()
	{
		krpt = new krptAES();
	}

	public void SetLocation(string loc)
	{
		Location = loc;
	}

	public void SetKey(string ky)
	{
		key = ky;
		truck1.GetComponent<TruckInfo>().truckName = ky;
	}

	public void Hide()
	{
		byte[] b = Encrypt();
		Split(b);
	}

	public byte[] Encrypt()
	{
		krptAES krptAES2 = new krptAES();
		string location = Location;
		key = truck1.GetComponent<TruckInfo>().truckName;
		byte[] array = krptAES2.AES_Encrypt(krptAES2.GetBytes(location), krptAES2.GetBytes(key));
		byte[] array2 = krptAES2.AES_Decrypt(array, krptAES2.GetBytes(key));
		return array;
	}

	public void Split(byte[] b)
	{
		lenght = b.Length;
		split = lenght / 3;
		int num = lenght % 3;
		byte[] array = new byte[split];
		byte[] array2 = new byte[split];
		byte[] array3 = new byte[split + num];
		int num2 = 0;
		int num3 = 0;
		while (num3 < split)
		{
			array[num3] = b[num2];
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < split)
		{
			array2[num3] = b[num2];
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < split + num)
		{
			array3[num3] = b[num2];
			num3++;
			num2++;
		}
		coche1.GetComponent<CarMixer>().carName = array;
		coche2.GetComponent<CarMixer>().carName = array2;
		coche3.GetComponent<CarMixer>().carName = array3;
	}

	public string Decript()
	{
		krptAES krptAES2 = new krptAES();
		key = truck1.GetComponent<TruckInfo>().truckName;
		byte[] carName = coche1.GetComponent<CarMixer>().carName;
		byte[] carName2 = coche2.GetComponent<CarMixer>().carName;
		byte[] carName3 = coche3.GetComponent<CarMixer>().carName;
		byte[] array = new byte[carName.Length + carName2.Length + carName3.Length];
		carName.CopyTo(array, 0);
		carName2.CopyTo(array, carName.Length);
		carName3.CopyTo(array, carName2.Length + carName.Length);
		byte[] bytes = krptAES2.AES_Decrypt(array, krptAES2.GetBytes(key));
		return krptAES2.GetString(bytes);
	}
}
