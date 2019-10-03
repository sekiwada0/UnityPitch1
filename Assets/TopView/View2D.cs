
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class View2D : MonoBehaviour {
	void Start()
	{
		m_draw = GetComponent<PVADraw>();
		m_draw.setAntiAliasing( true );
	}
	void Update()
	{
	}

	public void setSize(int width,int height)
	{
		m_draw.setSize( width, height );
	}
	public void setColor(Color color)
	{
		m_color = color;
	}
	public void setWidth(float w)
	{
		m_draw.begin();
		m_draw.setWidth( w );
		m_draw.end();
	}
	public void setViewZoom(float value)
	{
		m_draw.setViewZoom( value );
	}
	public void setViewOffset(Vector2 value)
	{
		m_draw.setViewOffset( value );
	}

	Vector2 getVector2(Vector3 pos,bool viewTop)
	{
		Vector2 pos2D;
		if( viewTop )
			pos2D = new Vector2(pos.x, pos.z);
		else
			pos2D = new Vector2(pos.z, pos.y);
		return pos2D;
	}
	public void clear(){
		m_draw.clear();
	}
	public void drawArray(List<Vector3> posArray,bool viewTop){
		int size = posArray.Count;
		if( size == 0 )
			return;

		m_draw.begin();
		m_draw.setColor( m_color );
		m_draw.moveTo( getVector2(posArray[0], viewTop) );
		for(int i = 1; i < size; i++){
			m_draw.lineTo( getVector2(posArray[i], viewTop) );
		}
		m_draw.end();
	}
	public void drawFadeArray(List<Vector3> posArray,bool viewTop){
		int size = posArray.Count;
		if( size == 0 )
			return;

		m_draw.begin();
		m_draw.moveTo( getVector2(posArray[0], viewTop) );
		for(int i = 1; i < size; i++){
			float alpha = (float)i / (float)size;
			m_draw.setColor( new Color(m_color.r, m_color.g, m_color.b, alpha) );
			m_draw.lineTo( getVector2(posArray[i], viewTop) );
		}
		m_draw.end();
	}
	public PVADraw getDraw(){ return m_draw; }
	private PVADraw m_draw;
	private Color m_color = new Color(1,1,1,1);
}
