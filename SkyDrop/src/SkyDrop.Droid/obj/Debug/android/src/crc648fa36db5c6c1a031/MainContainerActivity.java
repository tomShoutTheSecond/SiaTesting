package crc648fa36db5c6c1a031;


public class MainContainerActivity
	extends crc6426691c7e9794497c.BaseActivity_1
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkyDrop.Droid.Views.Main.MainContainerActivity, SkyDrop.Droid", MainContainerActivity.class, __md_methods);
	}


	public MainContainerActivity ()
	{
		super ();
		if (getClass () == MainContainerActivity.class)
			mono.android.TypeManager.Activate ("SkyDrop.Droid.Views.Main.MainContainerActivity, SkyDrop.Droid", "", this, new java.lang.Object[] {  });
	}


	public MainContainerActivity (int p0)
	{
		super (p0);
		if (getClass () == MainContainerActivity.class)
			mono.android.TypeManager.Activate ("SkyDrop.Droid.Views.Main.MainContainerActivity, SkyDrop.Droid", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
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
