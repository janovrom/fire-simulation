using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovement : MonoBehaviour
    {

        public bool zoomToCenter = false;
        public float lookSpeedH = 2f;
        public float lookSpeedV = 2f;
        public float zoomSpeed = 2f;
        public float dragSpeed = 6f;
        public float CrosshairPixelRadius = 5f;
        public GameObject CameraCrosshairPrefab;
        public LayerMask PickMask = ~0;
        public Vector3Variable CrosshairPosition;

        public float yaw;
        public float pitch;
        private Camera _camera;
        private GameObject _cameraCrosshair;


        private void Awake()
        {
            _camera = GetComponent<Camera>();

            // Set default values
            pitch = _camera.transform.eulerAngles.x;
            yaw = _camera.transform.eulerAngles.y;

            if (CameraCrosshairPrefab is object)
            {
                _cameraCrosshair = GameObject.Instantiate(CameraCrosshairPrefab);
                UpdateCrosshair();
            }
        }

        void Update()
        {
            UpdateCrosshair();
            LookAround();
            DragCamera();
            CameraZoom();
            SetYawPitch();
        }

        private void UpdateCrosshair()
        {
            var crosshairRay = _camera.ScreenPointToRay(Input.mousePosition);
            if (_cameraCrosshair is object && Physics.Raycast(crosshairRay, out RaycastHit hit, 10000f, PickMask))
            {
                _cameraCrosshair.transform.position = hit.point;
                Vector3 hitOnScreen = _camera.WorldToScreenPoint(hit.point);
                Vector3 projectedToWorld = _camera.ScreenToWorldPoint(hitOnScreen + Vector3.right * CrosshairPixelRadius);
                _cameraCrosshair.transform.localScale = Vector3.one * (projectedToWorld - hit.point).magnitude;
                
                if (CrosshairPosition is object)
                    CrosshairPosition.Value = hit.point;            
            }
        }

        private void LookAround()
        {
            if (Input.GetMouseButton(1))
            {

                pitch = _camera.transform.eulerAngles.x;
                yaw = _camera.transform.eulerAngles.y;

                yaw += lookSpeedH * Input.GetAxis("Mouse X");
                pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

            }
        }

        private void DragCamera()
        {
            if (Input.GetMouseButton(2))
            {
                transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed,
                    -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
            }
            else
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                transform.Translate(Time.deltaTime * dragSpeed * (Vector3.right * horizontal + Vector3.up * vertical));
            }
        }

        private void CameraZoom()
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                float speed = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
                if (zoomToCenter)
                {
                    transform.Translate(0, 0, speed, Space.Self);
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Physics.Raycast(ray, out _, 25);
                    Vector3 Scrolldirection = ray.GetPoint(5);

                    transform.position = Vector3.MoveTowards(transform.position, Scrolldirection, speed);
                }
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                float mod = Input.GetKey(KeyCode.LeftShift) ? -1f : 1f;

                transform.Translate(dragSpeed * mod * Time.deltaTime * Vector3.forward);
            }
        }

        private void SetYawPitch()
        {
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            yaw %= 360;
            pitch %= 360;
        }

    }

}
