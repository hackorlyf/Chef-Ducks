using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent{
    public static Player Instance{ get; private set; }

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs{
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] float rotateSpeed = 7f;
    [SerializeField] GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private Vector3 lastInteractionDir;
    private BaseCounter selectedCounter;    
    private KitchenObject kitchenObject;

    private bool isWalking;

    private void Awake(){
        if(Instance != null){
            Debug.LogError("There is more than one player instance");
        }
        Instance = this;
    }

    private void Start(){
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e){
        if(!GameManager.Instance.IsGamePlaying()) return;

        if(selectedCounter != null){
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e){
        if(!GameManager.Instance.IsGamePlaying()) return;
        
        if(selectedCounter != null){
            selectedCounter.Interact(this);
        }
    }

    private void Update(){
        HandleMovement();
        HandleInteraction();
    }

    private void HandleInteraction(){
        Vector2 inputVector = gameInput.GetMovementInputNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if(moveDir != Vector3.zero){
            lastInteractionDir = moveDir;
        }

        float interactDistance = 2f;
        if(Physics.Raycast(transform.position, lastInteractionDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)){
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)){
                // has clear counter
                if(baseCounter != selectedCounter){
                    SetSelectedCounter(baseCounter);
                }                
            }
            else{
                SetSelectedCounter(null);
            }
        }
        else{
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement(){
        Vector2 inputVector = gameInput.GetMovementInputNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = 0.65f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position,transform.position + Vector3.up*playerHeight,  playerRadius, moveDir, moveDistance);

        if(!canMove){
            //Can't move towards moveDir
            
            //Attempt only on X
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position,transform.position + Vector3.up*playerHeight,  playerRadius, moveDirX, moveDistance);

            if(canMove){
                //Can move only on the X
                moveDir = moveDirX;
            }
            else{
                //Can't move only on the x

                //Attempt on the z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position,transform.position + Vector3.up*playerHeight,  playerRadius, moveDirZ, moveDistance);

                if(canMove){
                    //Can move only on z
                    moveDir = moveDirZ;
                }
                else{
                    //Can't move in any direction
                }
            }

            
        }

        if(canMove){
            transform.position += moveDir * moveDistance;
        }

        isWalking = moveDir != Vector3.zero;

        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    public bool IsWalking(){
        return isWalking;
    }

    private void SetSelectedCounter(BaseCounter selectedCounter){
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs{
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform(){
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject(){
        return kitchenObject;
    }

    public void ClearKitchenObject(){
        kitchenObject = null;
    }

    public bool HasKitchenObject(){
        return kitchenObject != null;
    }
}
