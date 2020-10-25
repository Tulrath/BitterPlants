using System.Collections;

// GENERATED CODE.
public class TrackedBundleVersion
{
	public static readonly string bundleIdentifier = "com.lakehomegames.bitterplants";

	public static readonly TrackedBundleVersionInfo Version_1_0_61 =  new TrackedBundleVersionInfo ("1.0.61", 0);
	public static readonly TrackedBundleVersionInfo Version_1_0_71 =  new TrackedBundleVersionInfo ("1.0.71", 1);
	public static readonly TrackedBundleVersionInfo Version_1_0_73 =  new TrackedBundleVersionInfo ("1.0.73", 2);
	public static readonly TrackedBundleVersionInfo Version_1_0_74 =  new TrackedBundleVersionInfo ("1.0.74", 3);
	public static readonly TrackedBundleVersionInfo Version_1_0_75 =  new TrackedBundleVersionInfo ("1.0.75", 4);
	public static readonly TrackedBundleVersionInfo Version_1_0_76 =  new TrackedBundleVersionInfo ("1.0.76", 5);
	public static readonly TrackedBundleVersionInfo Version_1_0_77 =  new TrackedBundleVersionInfo ("1.0.77", 6);
	public static readonly TrackedBundleVersionInfo Version_1_0_78 =  new TrackedBundleVersionInfo ("1.0.78", 7);
	public static readonly TrackedBundleVersionInfo Version_1_0_79 =  new TrackedBundleVersionInfo ("1.0.79", 8);
	public static readonly TrackedBundleVersionInfo Version_1_0_82 =  new TrackedBundleVersionInfo ("1.0.82", 9);
	public static readonly TrackedBundleVersionInfo Version_1_0_84 =  new TrackedBundleVersionInfo ("1.0.84", 10);
	public static readonly TrackedBundleVersionInfo Version_1_0_85 =  new TrackedBundleVersionInfo ("1.0.85", 11);
	public static readonly TrackedBundleVersionInfo Version_1_0_87 =  new TrackedBundleVersionInfo ("1.0.87", 12);
	public static readonly TrackedBundleVersionInfo Version_1_0_88 =  new TrackedBundleVersionInfo ("1.0.88", 13);
	public static readonly TrackedBundleVersionInfo Version_1_0_89 =  new TrackedBundleVersionInfo ("1.0.89", 14);
	public static readonly TrackedBundleVersionInfo Version_1_0_95 =  new TrackedBundleVersionInfo ("1.0.95", 15);
	public static readonly TrackedBundleVersionInfo Version_1_0_106 =  new TrackedBundleVersionInfo ("1.0.106", 16);
	public static readonly TrackedBundleVersionInfo Version_1_0_107 =  new TrackedBundleVersionInfo ("1.0.107", 17);
	public static readonly TrackedBundleVersionInfo Version_1_0_108 =  new TrackedBundleVersionInfo ("1.0.108", 18);
	public static readonly TrackedBundleVersionInfo Version_1_0_109 =  new TrackedBundleVersionInfo ("1.0.109", 19);
	public static readonly TrackedBundleVersionInfo Version_1_0_110 =  new TrackedBundleVersionInfo ("1.0.110", 20);
	public static readonly TrackedBundleVersionInfo Version_1_0_111 =  new TrackedBundleVersionInfo ("1.0.111", 21);
	public static readonly TrackedBundleVersionInfo Version_1_1_112 =  new TrackedBundleVersionInfo ("1.1.112", 22);
	public static readonly TrackedBundleVersionInfo Version_1_1_113 =  new TrackedBundleVersionInfo ("1.1.113", 23);
	public static readonly TrackedBundleVersionInfo Version_1_1_114 =  new TrackedBundleVersionInfo ("1.1.114", 24);
	public static readonly TrackedBundleVersionInfo Version_1_1_115 =  new TrackedBundleVersionInfo ("1.1.115", 25);
	public static readonly TrackedBundleVersionInfo Version_1_1_116 =  new TrackedBundleVersionInfo ("1.1.116", 26);
	public static readonly TrackedBundleVersionInfo Version_1_2_117 =  new TrackedBundleVersionInfo ("1.2.117", 27);
	
	public static readonly TrackedBundleVersion Instance = new TrackedBundleVersion ();

	public static TrackedBundleVersionInfo Current { get { return Instance.current; } }

	public static int CurrentAndroidBundleVersionCode { get { return 117; } }

	public ArrayList history = new ArrayList ();

	public TrackedBundleVersionInfo current = Version_1_2_117;

	public  TrackedBundleVersion() {
		history.Add (Version_1_0_61);
		history.Add (Version_1_0_71);
		history.Add (Version_1_0_73);
		history.Add (Version_1_0_74);
		history.Add (Version_1_0_75);
		history.Add (Version_1_0_76);
		history.Add (Version_1_0_77);
		history.Add (Version_1_0_78);
		history.Add (Version_1_0_79);
		history.Add (Version_1_0_82);
		history.Add (Version_1_0_84);
		history.Add (Version_1_0_85);
		history.Add (Version_1_0_87);
		history.Add (Version_1_0_88);
		history.Add (Version_1_0_89);
		history.Add (Version_1_0_95);
		history.Add (Version_1_0_106);
		history.Add (Version_1_0_107);
		history.Add (Version_1_0_108);
		history.Add (Version_1_0_109);
		history.Add (Version_1_0_110);
		history.Add (Version_1_0_111);
		history.Add (Version_1_1_112);
		history.Add (Version_1_1_113);
		history.Add (Version_1_1_114);
		history.Add (Version_1_1_115);
		history.Add (Version_1_1_116);
		history.Add (current);
	}

}
