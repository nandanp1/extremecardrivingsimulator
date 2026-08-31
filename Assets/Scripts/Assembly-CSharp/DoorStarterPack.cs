using UnityEngine;

public class DoorStarterPack : MonoBehaviour
{
	private bool wasDragging;

	public UISprite fade;

	public UISprite fade2;

	public UISprite fade3;

	public UISprite fadeShadow;

	public AudioSource openingDoor;

	public AudioSource fallingDoor;

	public AudioSource hitDoor;

	public AudioSource autoOpen;

	public TweenAlpha flash;

	public UIPanel root;

	public UISprite shadowBot;

	public UISprite arrow;

	private bool canPlayUpAndDown;

	private bool wasMoved;

	private bool openingSoundIsPlaying;

	public bool isRealMoving;

	private bool moveUp;

	private bool alreadyEnter;

	private void OnEnable()
	{
		MySingleton<Manager>.Instance.guiMenuLogic.pnDoor = root;
	}

	private void OnPress(bool pressed)
	{
		if (pressed || !wasDragging)
		{
			return;
		}
		wasDragging = false;
		GetComponent<Rigidbody2D>().isKinematic = false;
		if (!moveUp)
		{
			openingDoor.Stop();
			openingSoundIsPlaying = false;
			if (!fallingDoor.isPlaying && wasMoved && canPlayUpAndDown)
			{
				fallingDoor.Play();
			}
		}
	}

	private void OnDrag(Vector2 delta)
	{
		wasMoved = true;
		wasDragging = true;
		GetComponent<Rigidbody2D>().isKinematic = true;
		fallingDoor.Stop();
		if (!openingDoor.isPlaying && canPlayUpAndDown)
		{
			openingDoor.Play();
			openingSoundIsPlaying = true;
		}
	}

	private void Update()
	{
		fade.alpha = Mathf.Lerp(1f, 0f, base.transform.localPosition.y / (float)fade.height);
		fade2.alpha = Mathf.Lerp(1f, 0f, base.transform.localPosition.y / (float)fade.height);
		fade3.alpha = Mathf.Lerp(1f, 0f, base.transform.localPosition.y / (float)fade.height);
		fadeShadow.alpha = Mathf.Lerp(1f, 0f, base.transform.localPosition.y / (float)fade.height);
		arrow.alpha = Mathf.Lerp(1f, 0f, base.transform.localPosition.y / (float)arrow.height);
		shadowBot.alpha = Mathf.Lerp(1f, 0f, base.transform.localPosition.y / (float)shadowBot.height);
		if (base.transform.position.y + 0.01f < Vector3.zero.y)
		{
			base.transform.position = Vector3.zero;
		}
		if (moveUp)
		{
			GetComponent<Rigidbody2D>().isKinematic = true;
			base.transform.Translate(Vector2.up * Time.deltaTime);
			if (base.transform.localPosition.y > 1000f)
			{
				hitDoor.Play();
				autoOpen.Stop();
				fallingDoor.Stop();
				openingSoundIsPlaying = false;
				NGUITools.SetActive(root.gameObject, false);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.name == "TriggerBot")
		{
			moveUp = true;
			Object.Destroy(GetComponent<BoxCollider2D>());
			Object.Destroy(GetComponent<UIDragObject>());
			GetComponent<Rigidbody2D>().isKinematic = false;
			openingDoor.Stop();
			autoOpen.Play();
			fallingDoor.Stop();
		}
		if (other.name == "TriggerSounds")
		{
			canPlayUpAndDown = true;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "TriggerHit")
		{
			if (alreadyEnter)
			{
				hitDoor.Play();
				openingDoor.Stop();
				fallingDoor.Stop();
				openingSoundIsPlaying = false;
			}
			else
			{
				alreadyEnter = true;
			}
		}
		if (other.name == "TriggerSounds")
		{
			canPlayUpAndDown = false;
		}
	}
}
