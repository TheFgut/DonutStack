using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    [SerializeField]
    internal Board board;
    public DonutGunColumns donutGunColumns;
    [SerializeField]
    internal donutsRandomizer donutsRandomModule;

    public static GameField instance;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        donutsRandomModule.Initialization();
        board.generateField();
    }





    [System.Serializable]
    public class DonutGunColumns
    {
        public MeshRenderer[] columns;

        public void gunOnLineEffect(int lineNum, int colorId)
        {
            for (int line = 0; line < columns.Length; line++)
            {
                if (lineNum == line)
                {
                    columns[line].enabled = true;
                    columns[line].material.color = GameField.instance.donutsRandomModule.getColorByID(colorId);
                }
                else
                {
                    columns[line].enabled = false;
                }
            }
        }

        public Vector3 getLineGunPos(int lineNum)
        {
            return new Vector3(columns[lineNum].transform.position.x,0, columns[lineNum].transform.position.z)
                + new Vector3(0,0,-(GameField.instance.board.heigth/2f + 1));
        }
    }

    [System.Serializable]
    internal class Board
    {
        public int width;
        public int heigth;

        public float distBetweenCells = 1;

        Cell[,] field;
        //init
        public void generateField()
        {
            field = new Cell[width, heigth];

            float etalonOffsetX = -width / 2f + 0.5f;
            float offsetY = -heigth / 2f + 0.5f;
            for (int Y = 0; Y < heigth; Y++)
            {
                float offsetX = etalonOffsetX;
                for (int X = 0; X < width; X++)
                {
   
                    Vector3 pos = new Vector3(offsetX * distBetweenCells,0,offsetY * distBetweenCells);
                    field[X, Y] = new Cell(X, Y, pos, this);
                    offsetX++;
                }
                offsetY++;
            }
        }

        //shooting
        public voidDelegateFun donutsShoot(DonutsPack donuts,int columnId,out Vector3 position)
        {
            if (field[columnId, 0].donutsInCell != null)
            {
                position = field[columnId, field.GetLength(1) - 1].position + new Vector3(0,0,-distBetweenCells);
                donuts.transform.position = position;
                LevelManager.manage.gameOver();
                return null;
            }
            for (int Y = 1; Y < heigth;Y++)
            {
                if (field[columnId, Y].donutsInCell != null)
                {
                    int prevY = Y - 1;
                    position = field[columnId, prevY].position;
                    return shootSuccess(field[columnId, prevY], donuts);
                }               
            }
            int lastY = field.GetLength(1) - 1;
            position = field[columnId, lastY].position;
            return shootSuccess(field[columnId, lastY], donuts);
        }

        voidDelegateFun shootSuccess(Cell cell, DonutsPack donuts)
        {
            cell.donutsInCell = donuts;
            return cell.checkMatches;
        }


        //match finding
        private Cell[] getNeighbourCells(Cell cell)
        {
            List<Cell> neighbours = new List<Cell>();
            if (cell.X > 0)
            {
                neighbours.Add(field[cell.X - 1, cell.Y]);
            }
            if (cell.X < width - 1)
            {
                neighbours.Add(field[cell.X + 1, cell.Y]);
            }
            if (cell.Y < heigth - 1)
            {
                neighbours.Add(field[cell.X , cell.Y + 1]);
            }
            if (cell.Y > 0)
            {
                neighbours.Add(field[cell.X, cell.Y - 1]);
            }
            return neighbours.ToArray();
        }

        LinkedList<donutsTransferAction> getMatchesActions(Cell origin, out int score)
        {

            if (origin.donutsInCell == null)
            {
                score = 0;
                return new LinkedList<donutsTransferAction>();
            }
            int colorId = origin.donutsInCell.GetUpperColorId();
            Cell[] neighbours = getNeighbourCells(origin);
            List<Cell> neighboursMatch = new List<Cell>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i].donutsInCell != null &&
                    neighbours[i].donutsInCell.GetUpperColorId() == colorId)
                {
                    neighboursMatch.Add(neighbours[i]);
                }
            }


            int originSameColorCount = origin.donutsInCell.getSameColorElementsCount(colorId);
            int originEmptyCount = origin.donutsInCell.getEmptySpaces();

            int bestScore = 0;
            LinkedList<donutsTransferAction> bestActions = new LinkedList<donutsTransferAction>();
            for (int i = 0; i < neighboursMatch.Count; i++)
            {

                int emptySpaces = neighboursMatch[i].donutsInCell.getEmptySpaces();
                int sameColorCount = neighboursMatch[i].donutsInCell.getSameColorElementsCount(colorId);
                //rules
                if (originSameColorCount <= emptySpaces || neighboursMatch[i].donutsInCell.onlyOneColorCheck(colorId))
                {
                    //making changes
                    score = originSameColorCount;
                    score += emptySpaces - originSameColorCount;
                    int gotScore;
                    LinkedList<donutsTransferAction> actions = transferFromOneToAnother(origin,
                        neighboursMatch[i], origin, neighboursMatch[i], colorId, out gotScore);
                    score += gotScore;
                    if (score > bestScore)
                    {
                        bestActions = actions;
                        bestScore = score;
                    }
                }
                if (originEmptyCount >= sameColorCount || origin.donutsInCell.onlyOneColorCheck(colorId))
                {
                    score = sameColorCount;
                    score += originEmptyCount - sameColorCount;
                    int gotScore;
                    LinkedList<donutsTransferAction> actions = transferFromOneToAnother(origin,
                        neighboursMatch[i],neighboursMatch[i], origin,colorId ,out gotScore);
                    score += gotScore;
                    if (score > bestScore)
                    {
                        bestActions = actions;
                        bestScore = score;
                    }
                }

            }

            score = bestScore;
            return bestActions;
        }

        private LinkedList<donutsTransferAction> transferFromOneToAnother(Cell origin,Cell target,Cell from, Cell to,int colorId, out int score)
        {
            //making changes
            score = 0;
            List<donutsTransferAction> actionsPrevious = new List<donutsTransferAction>();
            actionsPrevious.Add(new changeAction(from, to, colorId));
            actionsPrevious[actionsPrevious.Count - 1].MakeActionSafe();
            int destroyScore;
            if (from.donutsInCell.safeDestroyCheck(out destroyScore))
            {
                score += destroyScore;
                actionsPrevious.Add(new MoveUpAction(from,field));
                actionsPrevious[actionsPrevious.Count - 1].MakeActionSafe();
            }
            if (to.donutsInCell != null && to.donutsInCell.safeDestroyCheck(out destroyScore))
            {
                score += destroyScore;
                actionsPrevious.Add(new MoveUpAction(to,field));
                actionsPrevious[actionsPrevious.Count - 1].MakeActionSafe();
            }
            else if (from.donutsInCell != null && from.donutsInCell.safeDestroyCheck(out destroyScore))
            {
                score += destroyScore;
                actionsPrevious.Add(new MoveUpAction(from, field));
                actionsPrevious[actionsPrevious.Count - 1].MakeActionSafe();
            }

            int gotScore;
            LinkedList<donutsTransferAction> bestActions = getMatchesActions(target, out gotScore);
            int newScore;
            LinkedList<donutsTransferAction> actions = getMatchesActions(origin, out newScore);
            if (newScore >= gotScore)
            {
                bestActions = actions;
                gotScore = newScore;
            }
            score += gotScore;
            //reversing changes and adding actions
            for (int a = actionsPrevious.Count - 1; a >= 0; a--)
            {
                bestActions.AddFirst(actionsPrevious[a]);
                actionsPrevious[a].reverseActionSafe();
            }

            return bestActions;
        }

        //private LinkedList<donutsTransferAction> 
        abstract class donutsTransferAction
        {
            internal abstract void MakeActionSafe();
            internal abstract void reverseActionSafe();

            internal abstract void MakeAction();
            internal abstract void MakeActionAnim(float coef);

            public abstract int getTransferCount();
        }
        class changeAction : donutsTransferAction
        {
            public Cell from { get; private set; }
            private DonutsPack donutsPackFrom;
            public Cell to { get; private set; }
            private DonutsPack donutsPackTo;
            public int colorId { get; private set; }


            public changeAction(Cell from, Cell to,int colorId)
            {
                this.from = from;
                this.to = to;
                this.colorId = colorId;
                donutsPackFrom = from.donutsInCell;
                donutsPackTo = to.donutsInCell;
            }

            //safe manage(for best matches founding algoritm)
            internal override void MakeActionSafe()
            {
                int countTaken = 0;

                int sameColorCountOrig = donutsPackFrom.getSameColorElementsCount(colorId);
                int emptySpacesEnd = donutsPackTo.getEmptySpaces();
                int dif = emptySpacesEnd - sameColorCountOrig;
                if (dif < 0)
                {
                    countTaken = emptySpacesEnd;
                }
                else
                {
                    countTaken = sameColorCountOrig;
                }

                transfer = donutsPackFrom.safeRemoveDonutsCount(countTaken);

                donutsPackTo.safeAddDonuts(transfer);
            }

            internal override void reverseActionSafe()
            {
                transfer = donutsPackTo.safeRemoveDonutsCount(transfer.Length);
                donutsPackFrom.safeAddDonuts(transfer);

            }

            Donut[] transfer;
            public override int getTransferCount()
            {
                return transfer.Length;
            }
            //moving animation
            Vector3[] startPoints;
            float[] betweenElevation;
            Vector3[] destinationPoints;
            internal override void MakeAction()
            {

                transfer = donutsPackFrom.safeRemoveDonutsCount(transfer.Length);

                startPoints = new Vector3[transfer.Length];
                betweenElevation = new float[transfer.Length];
                destinationPoints = new Vector3[transfer.Length];
                
                int drag = 3 - to.donutsInCell.getEmptySpaces();
                for (int i = 0; i < transfer.Length;i++)
                {
                    transfer[i].transform.parent = to.donutsInCell.transform;
                    startPoints[i] = transfer[i].transform.position;
                    float dist = (drag + i) * DonutsPack.distBetweenDonuts;
                    destinationPoints[i] = to.donutsInCell.transform.position + new Vector3(0, dist, 0);
                    betweenElevation[i] = 1.5f + dist;
                   
                }
                donutsPackTo.safeAddDonuts(transfer);

                if (donutsPackFrom.getEmptySpaces() == 3)
                {
                    donutsPackFrom.HideStick();
                }
                //setting anim params
            }
            
            internal override void MakeActionAnim(float coef)
            {
                float partHalf = 1f / transfer.Length;
                int id = Mathf.FloorToInt(coef / partHalf);
                if (id >= transfer.Length)
                {

                    id = transfer.Length - 1;
                }
                if (id < 0)
                {
                    id = 0;
                }
                print(id);
                float localCoef = (coef - (partHalf * id)) * transfer.Length;

                Vector3 elevation = new Vector3(0, Mathf.Lerp(0, betweenElevation[id], 1 - (Mathf.Abs(localCoef - 0.5f) * 2)), 0);
                transfer[id].transform.position = Vector3.Lerp(startPoints[id], destinationPoints[id], easings.easeOutQuart(localCoef)) + elevation;

            }
        }

        class MoveUpAction : donutsTransferAction
        {
            //двигает элементы в линии и запоминает что и куда подвинул
            //(а также удаленный объект - достает из €чейки и хранит у себ€ до реверса действи€)
            DonutsPack destroyedDonuts;

            Cell destroyed;
            Cell[,] field;

            int movedByAction;
            public MoveUpAction(Cell destroyed, Cell[,] field)
            {
                this.destroyed = destroyed;
                this.field = field;
            }

            internal override void MakeActionSafe()
            {
                movedByAction = 0;
                destroyedDonuts = destroyed.donutsInCell;
                int X = destroyed.X;
                int downY = -1;
                for (int y = destroyed.Y; y > 0; y--)
                {
                    downY = y - 1;
                    if (field[X, downY].donutsInCell != null)
                    {
                        field[X, y].donutsInCell = field[X, downY].donutsInCell;
                        movedByAction++;
                    }
                    else
                    {
                        break;
                    }
                }
                field[X, downY + 1].donutsInCell = null;
            }

            internal override void reverseActionSafe()
            {
                int X = destroyed.X;
                for (int y = 0; y < destroyed.Y; y++)
                {
                    field[X, y].donutsInCell = field[X, y + 1].donutsInCell;
                }
                destroyed.donutsInCell = destroyedDonuts;
            }

            DonutsPack[] movedDonuts;
            Vector3[] startPositions;
            Vector3[] destinationPositions;

            internal override void MakeAction()
            {
                int Counter = 0;
                movedDonuts = new DonutsPack[movedByAction];
                startPositions = new Vector3[movedByAction];
                destinationPositions = new Vector3[movedByAction];
                int X = destroyed.X;
                destroyed.donutsInCell.removeAndDestroy();
                destroyed.donutsInCell = null;
                for (int y = destroyed.Y; y > 0; y--)
                {
                    int downY = y - 1;
                    if (field[X, downY].donutsInCell != null)
                    {
                        field[X, y].donutsInCell = field[X, downY].donutsInCell;
                        movedDonuts[Counter] = field[X, downY].donutsInCell;
                        startPositions[Counter] = movedDonuts[Counter].transform.position;
                        destinationPositions[Counter] = startPositions[Counter] 
                            + new Vector3(0,0, GameField.instance.board.distBetweenCells);
                        Counter++;
                    }
                    else
                    {
                        field[X, y].donutsInCell = null;
                        break;
                    }
                }
            }

            internal override void MakeActionAnim(float coef)
            {
                for (int i = 0; i< movedDonuts.Length;i++)
                {
                    movedDonuts[i].transform.position = Vector3.Lerp(startPositions[i],
                        destinationPositions[i],easings.easeOutQuad(coef));
                }
            }

            public override int getTransferCount()
            {
                return 1;
            }
        }

        class Cell
        {
            public int X;
            public int Y;

            public DonutsPack donutsInCell;
            Board board;

            public Vector3 position;
            public Cell(int X, int Y,Vector3 pos, Board board)
            {
                this.X = X;
                this.Y = Y;
                position = pos;
                this.board = board;
            }

            public void checkMatches(voidDelegate finish)
            {
                int score;
                LinkedList<donutsTransferAction> actions = board.getMatchesActions(this, out score);

                GameField.instance.StartCoroutine(donutsActionsRunner(actions, finish));
            }

            IEnumerator donutsActionsRunner(LinkedList<donutsTransferAction> actions, voidDelegate finish)
            {
                float defaultAnimSpeed = 3;
                foreach (donutsTransferAction action in actions)
                {
                    float coef = 0;
                    action.MakeAction();
                    float currentAnimSpd = defaultAnimSpeed / action.getTransferCount();
                    Score.instance.IncreaseScore(100);
                    do
                    {
                        coef += Time.fixedDeltaTime * currentAnimSpd;
                        action.MakeActionAnim(coef);
                        yield return new WaitForFixedUpdate();
                    } while (coef < 1);
                }

                finish();
            }

        }
    }

}

public delegate void voidDelegate();
public delegate void voidDelegateFun(voidDelegate func);
public delegate void voidDelegateFLoat(float coef);
public static class easings
{
    public static float easeOutQuart(float x)
    {
        return 1 - Mathf.Pow(1 - x, 4);
    }

    public static float easeOutQuad(float x)
    {
        return 1 - (1 - x) * (1 - x);
    }

    public static float easeOutElastic(float x)
    {
        const float c4 = (2 * Mathf.PI) / 3;

        return Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
    }
}
