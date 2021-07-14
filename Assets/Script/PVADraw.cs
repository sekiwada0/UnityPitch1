
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class PVADraw : MonoBehaviour {
struct PVAVector2D {
	public float x, y;
};
struct PVAColor {
	public float r, g, b, a;
};

	[DllImport("PVADraw")]
	private static extern IntPtr PVADraw_create();

	[DllImport("PVADraw")]
	private static extern void PVADraw_destroy(IntPtr inst);

	[DllImport("PVADraw")]
	private static extern void PVADraw_setSize(IntPtr inst,IntPtr data,int w,int h);

	[DllImport("PVADraw")]
	private static extern int PVADraw_update(IntPtr inst);

	[DllImport("PVADraw")]
	private static extern void PVADraw_begin(IntPtr inst);

	[DllImport("PVADraw")]
	private static extern void PVADraw_end(IntPtr inst);

	[DllImport("PVADraw")]
	private static extern void PVADraw_setColor(IntPtr inst,ref PVAColor color);

	[DllImport("PVADraw")]
	private static extern void PVADraw_setWidth(IntPtr inst,float w);

	[DllImport("PVADraw")]
	private static extern void PVADraw_clear(IntPtr inst);

	[DllImport("PVADraw")]
	private static extern void PVADraw_moveTo(IntPtr inst,ref PVAVector2D pos);

	[DllImport("PVADraw")]
	private static extern void PVADraw_lineTo(IntPtr inst,ref PVAVector2D pos);

	[DllImport("PVADraw")]
	private static extern void PVADraw_setRadius(IntPtr inst,float radius,int screen);

	[DllImport("PVADraw")]
	private static extern void PVADraw_circle(IntPtr inst,int fill);

	[DllImport("PVADraw", CharSet = CharSet.Ansi)]
	private static extern void PVADraw_setBool(IntPtr inst,string name,int value);

	[DllImport("PVADraw", CharSet = CharSet.Ansi)]
	private static extern int PVADraw_getBool(IntPtr inst,string name);

	[DllImport("PVADraw", CharSet = CharSet.Ansi)]
	private static extern void PVADraw_setFloat(IntPtr inst,string name,float value);

	[DllImport("PVADraw", CharSet = CharSet.Ansi)]
	private static extern float PVADraw_getFloat(IntPtr inst,string name);

	[DllImport("PVADraw", CharSet = CharSet.Ansi)]
	private static extern void PVADraw_setVector(IntPtr inst,string name,ref PVAVector2D value);

	[DllImport("PVADraw", CharSet = CharSet.Ansi)]
	private static extern void PVADraw_getVector(IntPtr inst,string name,ref PVAVector2D value);

	[DllImport("PVADraw")]
	private static extern int PVADraw_ScreenToMarker(IntPtr inst,ref PVAVector2D wnd,ref PVAVector2D log);

	[DllImport("PVADraw")]
	private static extern int PVADraw_MarkerToScreen(IntPtr inst,ref PVAVector2D log,ref PVAVector2D wnd);

	private IntPtr m_inst = IntPtr.Zero;
	private Texture2D m_texture;
	private Color32[] m_pixels;
	private GCHandle m_pixels_handle;
	private PVAColor m_color;
	private PVAVector2D m_pos;

	void Start()
	{
		m_color = new PVAColor();
		m_pos = new PVAVector2D();
		m_inst = PVADraw_create();
	}
	void OnDestroy(){
		if( m_texture != null )
			destroyTexture();
		if( m_inst != IntPtr.Zero )
		{
			PVADraw_destroy( m_inst );
			m_inst = IntPtr.Zero;
		}
	}
	void Update()
	{
		if( m_texture == null )
			return;
		if( PVADraw_update(m_inst) != 0 ){
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

		PVADraw_setSize(m_inst, pixels_ptr, width, height);
	}
	private void destroyTexture()
	{
		// GC 対象にする
		m_pixels_handle.Free();
		m_texture = null;
	}

	public void begin(){ PVADraw_begin(m_inst); }
	public void end(){ PVADraw_end(m_inst); }
	public void setColor(Color color)
	{
		m_color.r = color.r;
		m_color.g = color.g;
		m_color.b = color.b;
		m_color.a = color.a;
		PVADraw_setColor( m_inst, ref m_color );
	}
	public void setWidth(float w)
	{
		PVADraw_setWidth( m_inst, w );
	}
	public void clear()
	{
		PVADraw_clear(m_inst);
	}
	public void moveTo(Vector2 pos)
	{
		m_pos.x = pos.x;
		m_pos.y = pos.y;
		PVADraw_moveTo( m_inst, ref m_pos );
	}
	public void lineTo(Vector2 pos)
	{
		m_pos.x = pos.x;
		m_pos.y = pos.y;
		PVADraw_lineTo( m_inst, ref m_pos );
	}
	public void setRadius(float radius,bool screen)
	{
		PVADraw_setRadius( m_inst, radius, screen? 1: 0 );
	}
	public void circle(bool fill)
	{
		PVADraw_circle( m_inst, fill? 1: 0 );
	}

	public void setAntiAliasing(bool value)
	{
		PVADraw_setBool(m_inst, "AntiAliasing", value? 1: 0);
	}
	public void setViewZoom(float value)
	{
		PVADraw_setFloat(m_inst, "ViewZoom", value);
	}
	public void setViewOffset(Vector2 value)
	{
		m_pos.x = value.x;
		m_pos.y = value.y;
		PVADraw_setVector(m_inst, "ViewOffset", ref m_pos);
	}
	public Vector2 ScreenToMarker(Vector2 wnd)
	{
		m_pos.x = wnd.x;
		m_pos.y = wnd.y;
		PVADraw_ScreenToMarker( m_inst, ref m_pos, ref m_pos );
		return new Vector2( m_pos.x, m_pos.y );
	}
	public Vector2 MarkerToScreen(Vector2 log)
	{
		m_pos.x = log.x;
		m_pos.y = log.y;
		PVADraw_MarkerToScreen( m_inst, ref m_pos, ref m_pos );
		return new Vector2( m_pos.x, m_pos.y );
	}
}
