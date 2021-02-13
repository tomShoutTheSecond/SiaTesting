package crc648fa36db5c6c1a031;


public class MainFragment
	extends crc6426691c7e9794497c.BaseFragment_1
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkyDrop.Droid.Views.Main.MainFragment, SkyDrop.Droid", MainFragment.class, __md_methods);
	}


	public MainFragment ()
	{
		super ();
		if (getClass () == MainFragment.class)
			mono.android.TypeManager.Activate ("SkyDrop.Droid.Views.Main.MainFragment, SkyDrop.Droid", "", this, new java.lang.Object[] {  });
	}


	public MainFragment (int p0)
	{
		super (p0);
		if (getClass () == MainFragment.class)
			mono.android.TypeManager.Activate ("SkyDrop.Droid.Views.Main.MainFragment, SkyDrop.Droid", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
