using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovement : MonoBehaviour
    {

        public bool zoomToCenter = false;
        public float lookSpeedH = 2f;
        public float lookSpeedV = 2f;
        public float zoomSpeed = 2f;
        public float dragSpeed = 6f;
        public GameObject CameraCrosshair;
        public LayerMask PickMask;

        public float yaw;
        public float pitch;
        private Camera _camera;


        private void Awake()
        {
            _camera = GetComponent<Camera>();

            // Set default values
            pitch = _camera.transform.eulerAngles.x;
            yaw = _camera.transform.eulerAngles.y;
        }

        void Update()
        {
            var crosshairRay = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(crosshairRay, out RaycastHit hit, 1000f, PickMask))
            {
                CameraCrosshair.transform.position = hit.point;
            }

            //Look around with Right Mouse
            if (Input.GetMouseButton(1))
            {

                pitch = _camera.transform.eulerAngles.x;
                yaw = _camera.transform.eulerAngles.y;

                yaw += lookSpeedH * Input.GetAxis("Mouse X");
                pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

            }

            //drag camera around with Middle Mouse
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

            //Zoom in and out with Mouse Wheel
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
                    RaycastHit point;
                    Physics.Raycast(ray, out point, 25);
                    Vector3 Scrolldirection = ray.GetPoint(5);

                    transform.position = Vector3.MoveTowards(transform.position, Scrolldirection, speed);
                }
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                float mod = Input.GetKey(KeyCode.LeftShift) ? -1f : 1f;

                transform.Translate(mod * Vector3.forward * dragSpeed * Time.deltaTime);
            }

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            yaw = yaw % 360;
            pitch = pitch % 360;
        }

    }

}
