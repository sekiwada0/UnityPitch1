
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class PVATGraph : MonoBehaviour {

	[SerializeField]
	private int width = 512;

	[SerializeField]
	private int height = 512;

	[DllImport("PVAT")]
	private static extern void PVAT_setGraphSize(IntPtr data,int w,int h);

	[DllImport("PVAT")]
	private static extern int PVAT_updateGraph();

	private Texture2D m_texture;
	private Color32[] m_pixels;
	private GCHandle m_pixels_handle;

	void Start()
	{
		setSize( width, height );
	}
	void OnDestroy(){
		if( m_texture != null )
			destroyTexture();
	}
	void Update()
	{
		if( m_texture == null )
			return;
		if( PVAT_updateGraph() != 0 ){
			m_texture.SetPixels32( m_pixels );
			m_texture.Apply();
		}
	}
	public void setSize(int width,int height)
	{
		if( m_texture != null )
			destroyTexture();
		createTexture( width, height );
	}

	private void createTexture(int width,int height)
	{
		//m_texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
		m_texture = new Texture2D(width, height);

		RawImage rawImage = GetComponent<RawImage>();
		rawImage.texture = m_texture;
		rawImage.material.mainTexture = m_texture;
		rawImage.color = new Color(1, 1, 1, 1);
		//rawImage.color = new Color(1, 1, 1, 0.5f);
		//rawImage.SetNativeSize();

		// Color32 型の配列としてテクスチャの参照をもらう
		m_pixels = m_texture.GetPixels32();

		// GC されないようにする
		m_pixels_handle = GCHandle.Alloc(m_pixels, GCHandleType.Pinned);

		// そのテクスチャのアドレスをもらう
		IntPtr pixels_ptr = m_pixels_handle.AddrOfPinnedObject();

		PVAT_setGraphSize(pixels_ptr, width, height);
	}
	private void destroyTexture()
	{
		// GC 対象にする
		m_pixels_handle.Free();
		m_texture = null;
	}
}
