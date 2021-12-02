using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class PrefabInst : MonoBehaviour
{
    public ARRaycastManager m_RaycastManager;
    public List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private RaycastHit raycastHit;
    private Vector2 center;

    public GameObject Marker;

    public GameObject _theBall;
    public Transform _camObj;
    public Transform _shootPoint;

    public Camera arCamera;
    public GameObject placeObject;
    private GameObject pObj;
    private GameObject bObj;
    private GameObject tObj;
    public Button setButton;
    private bool Touched = false;

    float time;
    float checkTime;
    private bool canMake = false;
    private bool canShoot = false;
    private bool isFirst = true;
    //public GameObject rayPoint;

    public bool isDelay;
    public float delayTime = 1f;

    Vector3 touchPos;
    public ThrowingObject throwingObject;
    public Transform parentOnThrow;
    public Vector3 inputPositionCurrent;
    private Vector3 inputPositionPivot;

    [Header("Throw")]
    [Tooltip("Actual for FPS Controller")]
    public bool isInputPositionFixed = false;

    [Range(0.01f, 1f)]
    public float inputPositionFixedScreenFactorX = 0.48f;

    [Range(0.01f, 1f)]
    public float inputPositionFixedScreenFactorY = 0.52f;
    public Vector2 inputSensitivity = new Vector2(1f, 100f);

    public float forceFactorExtra = 10f;
    public float torqueFactorExtra = 60f;
    public float torqueAngleExtra;

    [Header("FPS (throw force takes into account the speed of the player's movement) ")]
    public CharacterController characterControllerFPS;
    private float characterControllerFPSSpeedCurrent = 0f;

    public bool isReset = false;
    public GameObject finDescribe;

    // 터치, 드래그 구분 관련 변수
    Vector2 startPos, deltaPos, nowPos;
    bool isDragged;
    const float dragAccuracy = 50f;
    // Start is called before the first frame update
    void Start()
    {
        setButton.gameObject.SetActive(false);
        center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        time = 0.0f;
        checkTime = 3.0f;
        isDragged = false;
    }
    void Update()
    {
        // 공이 허스키 맞췄을 때 
        if (bObj)
        {
            if (bObj.GetComponent<CollisionDetect>().isNext)
            {
                canShoot = false;
                isFirst = true;
                Debug.Log("isNext");
                if (bObj.GetComponent<CollisionDetect>().partO.GetComponent<ParticleSystem>().isStopped)
                {
                    Debug.Log("1");
                    finDescribe.SetActive(true);
                    bObj.GetComponent<CollisionDetect>().isNext = false;
                }
            }
        }
        // raycast를 통한 평면 인식
        Ray ray;
        RaycastHit hitobj;

        // 마커가 생성되지 않았을때
        if (!canMake)
        {
            ray = arCamera.ScreenPointToRay(center);
            if (Physics.Raycast(ray, out hitobj))
            {
                if (m_RaycastManager.Raycast(ray, hits, TrackableType.Planes))
                {
                    time += Time.deltaTime * 1.5f;

                    if (tObj)
                    {
                        tObj.transform.position = hits[0].pose.position;
                        tObj.transform.rotation = hits[0].pose.rotation;
                    }
                    else
                    {
                        // 3초이상 평면을 비추면 마커 생성
                        if (time >= checkTime)
                        {
                            time = 0.0f;
                            tObj = Instantiate(Marker, hits[0].pose.position, hits[0].pose.rotation);
                            tObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                            setButton.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        // 마커가 생성되었을 때
        else
        {
            if (Input.touchCount == 0)
            {
                return;
            }
            Touch touch = Input.GetTouch(0);
            // Touch가 인식되면 ray를 쏴서 마커 생성
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                ray = arCamera.ScreenPointToRay(touch.position);
                // Ray를 통해 오브젝트 인식
                if (Physics.Raycast(ray, out hitobj))
                {
                    // 마커를 터치하면
                    if (hitobj.collider.gameObject.name.Contains("Marker"))
                    {
                        // 오브젝트 생성
                        isFirst = true;
                        pObj = Instantiate(placeObject, tObj.transform.position, tObj.transform.rotation);
                        pObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                        pObj.transform.rotation = new Quaternion(0f, -180f, 0f, 0f);
                        Destroy(tObj);
                        canShoot = true;
                    }
                }
            }
            if (canShoot)
            {
                if (isFirst) isFirst = false;
                else
                {
                    ShootBallObj();
                }
            }
        }
        
    }
    #region 공날리기
    public void ShootBallObj()
    {
        if (Input.touchCount == 0)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);

        nowPos = (Input.touchCount == 0) ? (Vector2)Input.mousePosition : touch.position;

        Ray ray;
        RaycastHit hitobj;

        inputPositionCurrent =
                    isInputPositionFixed
                    ? GetInputPositionFixed()
                    : Input.mousePosition;

        if (isDragged && bObj)
        {
            bObj.transform.position = _shootPoint.transform.position;
        }
        
        if (touch.phase == UnityEngine.TouchPhase.Began)
        {
            startPos = nowPos;
            //bObj = Instantiate(_theBall, hitobj.point, hitobj.transform.rotation);
            bObj = Instantiate(_theBall, _shootPoint.transform.position, _shootPoint.transform.rotation);

            bObj.transform.SetParent(parentOnThrow);
            inputPositionPivot = inputPositionCurrent;
        }
        if (touch.phase == UnityEngine.TouchPhase.Moved)
        {
            deltaPos = startPos - nowPos;

            if(deltaPos.sqrMagnitude > dragAccuracy)
            {
                isDragged = true;
                Debug.Log("isDragged");
            }
        }
        if (touch.phase == UnityEngine.TouchPhase.Ended)
        {
            if (isDragged)
            {
                if (characterControllerFPS)
                {
                    characterControllerFPSSpeedCurrent =
                        characterControllerFPS.transform.InverseTransformDirection(characterControllerFPS.velocity).z;
                }

                bObj.GetComponent<ThrowingObject>().ResetPosition(arCamera);
                bObj.GetComponent<ThrowingObject>().ResetRotation(_shootPoint);
                bObj.GetComponent<ThrowingObject>().Throw(
                    inputPositionPivot,
                    inputPositionCurrent,
                    inputSensitivity,
                    arCamera.transform,
                    Screen.height,
                    forceFactorExtra + characterControllerFPSSpeedCurrent,
                    torqueFactorExtra,
                    torqueAngleExtra);
                
            }
            else
            {
                bObj.transform.localScale = new Vector3(0f, 0f, 0f);
                bObj = null;
            }
            isDragged = false;
        }
    }
    #endregion
    private Vector3 GetInputPositionFixed()
    {
        return new Vector3(
            Screen.width * inputPositionFixedScreenFactorX,
            Screen.height * inputPositionFixedScreenFactorY,
            0f);
    }
    public void Reset()
    {
        if (tObj) Destroy(tObj);
        if (pObj) Destroy(pObj);
        for(int i = 0; i < parentOnThrow.childCount; i++)
        {
            Destroy(parentOnThrow.GetChild(i).gameObject);
        }
        if (parentOnThrow.childCount > 0) parentOnThrow.DetachChildren();
        canMake = false;
        canShoot = false;
        isReset = false;
        time = 0.0f;
        Touched = false;
    }
    public void SetMark()
    {
        canMake = true;
        setButton.gameObject.SetActive(false);
    }
}


