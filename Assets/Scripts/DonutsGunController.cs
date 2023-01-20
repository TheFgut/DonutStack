using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DonutsGunController : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    Camera cam;
    [SerializeField]
    private Gun gun;

    public DonutsPack packInGun;
    private bool controlsLock;
    // Start is called before the first frame update
    void Start()
    {
        gun.controller = this;
        gun.Reload(GameField.instance.donutsRandomModule.getRandomDonutPack());
        cam = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        chooseLine(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        chooseLine(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        chooseLine(eventData.position);
        if (controlsLock == true)
        {
            return;
        }
        gun.shootCommand();
    }

    internal int choosedColumn;
    public void chooseLine(Vector2 pos)
    {
        if (controlsLock == true)
        {
            return;
        }
        Ray ray = cam.ScreenPointToRay(pos);

        RaycastHit hit;
        Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("ColumnTrigger"));
        if (hit.collider != null)
        {
            if (gun.doShoot == true)
            {
                return;
            }
            choosedColumn = int.Parse(hit.collider.name.Substring(3));
            GameField.instance.donutGunColumns.gunOnLineEffect(choosedColumn, packInGun.GetUpperColorId());
            gun.MoveTo(choosedColumn);
        }
        else
        {
            gun.cancelShootCommand();
        }
    }

    public void LockControls()
    {
        controlsLock = true;
    }
    public void UnlockControls()
    {
        controlsLock = false;
    }

    void stopAll()
    {
        StopAllCoroutines();
    }



    [System.Serializable]
    class Gun
    {
        internal DonutsGunController controller; 
        public GameObject gunObj;

        public float basicTransferTime = 1;
        public float distAffectOnTransferTime = 0.5f;

        public float donutsFlySpeed;

        public bool doShoot { get; private set; }
        //shooting
        public void shootCommand()
        {

            doShoot = true;
            if (movingRoutine == null)
            {
                shoot();
            }
        }

        void shoot()
        {
            if (shootPocessRoutine != null)
            {
                return;
            }
            shootPocessRoutine = controller.StartCoroutine(shootProcess());
            controller.LockControls();
        }

        Coroutine shootPocessRoutine;

        IEnumerator shootProcess()
        {


            Vector3 startPos = gunObj.transform.position;
            Vector3 destination;
            voidDelegateFun cellFunc = GameField.instance.board.donutsShoot(controller.packInGun,
                currentLineID, out destination);
            if (cellFunc == null)
            {
                controller.LockControls();
                controller.stopAll();
                yield return null;
            }
            float coef = 0;
            float flyTime = (startPos - destination).magnitude / donutsFlySpeed;

            controller.choosedColumn = 2;
            MoveTo(2);
            GameField.instance.donutGunColumns.gunOnLineEffect(-1,0);
            controller.packInGun.Shake(flyTime);
            do
            {
                coef += Time.fixedDeltaTime / flyTime;
                controller.packInGun.transform.position = 
                    Vector3.Lerp(startPos, destination, easings.easeOutQuart(coef));
                yield return new WaitForFixedUpdate();
            } while (coef < 1);
            
            controller.packInGun.transform.parent = gunObj.transform.parent;
            Reload(GameField.instance.donutsRandomModule.getRandomDonutPack());
            cellFunc(controlsActivate);
            shootPocessRoutine = null;
            doShoot = false;
        }

        void controlsActivate()
        {
            controller.UnlockControls();
        }

        public void cancelShootCommand()
        {
            if (shootPocessRoutine != null)
            {
                return;
            }
            doShoot = false;
        }

        //reload
        public void Reload(DonutsPack newPack)
        {
            controller.packInGun = newPack;
            newPack.transform.parent = gunObj.transform;
            newPack.transform.localPosition = new Vector3();

        }

        //moving
        Vector3 startPos;
        Coroutine movingRoutine;
        int currentLineID = 2;
        public void MoveTo(int lineId)
        {
            
            if (currentLineID == lineId)
            {
                return;
            }
            if (movingRoutine != null)
            {
                controller.StopCoroutine(movingRoutine);
            }
            startPos = gunObj.transform.position;
            movingRoutine = controller.StartCoroutine(
                moving(GameField.instance.donutGunColumns.getLineGunPos(lineId)));
            currentLineID = lineId;
        }

        IEnumerator moving(Vector3 destination)
        {
            float coef = 0;
            float distCoef = (startPos - destination).magnitude / distAffectOnTransferTime;

            do
            {
                coef += (Time.fixedDeltaTime / basicTransferTime) * (1 + distCoef);

                gunObj.transform.position = 
                    Vector3.Lerp(startPos, destination, easings.easeOutElastic(coef));
                yield return new WaitForFixedUpdate();
            } while (coef < 0.5f);
            gunObj.transform.position = destination;
            movingRoutine = null;
            if (controller.choosedColumn != currentLineID)
            { 
                MoveTo(controller.choosedColumn);
            }
            else if(doShoot)
            {
                shoot();
            }

        }
    }
}
