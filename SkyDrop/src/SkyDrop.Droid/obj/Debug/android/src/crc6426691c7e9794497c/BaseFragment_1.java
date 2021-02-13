package crc6426691c7e9794497c;


public abstract class BaseFragment_1
	extends crc64db62d61d9af52c56.MvxFragment_1
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreateView:(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;:GetOnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("SkyDrop.Droid.Views.BaseFragment`1, SkyDrop.Droid", BaseFragment_1.class, __md_methods);
	}


	public BaseFragment_1 ()
	{
		super ();
		if (getClass () == BaseFragment_1.class)
			mono.android.TypeManager.Activate ("SkyDrop.Droid.Views.BaseFragment`1, SkyDrop.Droid", "", this, new java.lang.Object[] {  });
	}


	public BaseFragment_1 (int p0)
	{
		super (p0);
		if (getClass () == BaseFragment_1.class)
			mono.android.TypeManager.Activate ("SkyDrop.Droid.Views.BaseFragment`1, SkyDrop.Droid", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}


	public android.view.View onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2)
	{
		return n_onCreateView (p0, p1, p2);
	}

	private native android.view.View n_onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2);

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
