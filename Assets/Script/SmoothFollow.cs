using UnityEngine;

//namespace UnityStandardAssets.Utility
//{
	public class SmoothFollow : MonoBehaviour
	{
		private GameObject m_target;
		private Vector3 m_offset = new Vector3();

		// The distance in the x-z plane to the target
		[SerializeField]
		private float distance = 10.0f;

		// the height we want the camera to be above the target
		[SerializeField]
		private float height = 5.0f;

		[SerializeField]
		private float rotationDamping;

		[SerializeField]
		private float heightDamping;

		// Use this for initialization
		void Start() { }

		// Update is called once per frame
		public void update()
		{
			// Early out if we don't have a target
			if( !m_target )
				return;
			Transform target = m_target.transform;

			var currentHeight = transform.position.y;
			var wantedHeight = target.position.y + height;
			currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

			Vector3 position = new Vector3();
			position = target.position - m_offset;
			position.y = currentHeight;
			transform.position = position;

			transform.LookAt(target);
		}
		public void setTarget(GameObject obj)
		{
			m_target = obj;
			if( m_target != null ){
				Rigidbody rb = obj.GetComponent<Rigidbody>();
				m_offset = rb.velocity;
				m_offset.Normalize();
				m_offset *= distance;
			}
		}
		public GameObject getTarget()
		{
			return m_target;
		}
	}
//}
