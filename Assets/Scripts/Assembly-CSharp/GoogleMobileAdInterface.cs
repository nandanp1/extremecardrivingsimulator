using System.Collections.Generic;
using UnityEngine;

public interface GoogleMobileAdInterface
{
	List<GoogleMobileAdBanner> banners { get; }

	bool IsInited { get; }

	string BannersUunitId { get; }

	string InterstisialUnitId { get; }

	void Init(string ad_unit_id);

	void Init(string banners_unit_id, string interstisial_unit_id);

	void SetBannersUnitID(string ad_unit_id);

	void SetInterstisialsUnitID(string ad_unit_id);

	void AddKeyword(string keyword);

	void AddTestDevice(string deviceId);

	void SetGender(GoogleGenger gender);

	void SetBirthday(int year, AndroidMonth month, int day);

	void TagForChildDirectedTreatment(bool tagForChildDirectedTreatment);

	GoogleMobileAdBanner CreateAdBanner(TextAnchor anchor, GADBannerSize size);

	GoogleMobileAdBanner CreateAdBanner(int x, int y, GADBannerSize size);

	void DestroyBanner(int id);

	void StartInterstitialAd();

	void LoadInterstitialAd();

	void ShowInterstitialAd();

	void RecordInAppResolution(GADInAppResolution resolution);

	GoogleMobileAdBanner GetBanner(int id);

	void addEventListener(string eventName, EventHandlerFunction handler);

	void addEventListener(string eventName, DataEventHandlerFunction handler);

	void removeEventListener(string eventName, EventHandlerFunction handler);

	void removeEventListener(string eventName, DataEventHandlerFunction handler);
}
