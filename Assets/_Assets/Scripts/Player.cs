using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs{
        public ClearCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] float rotateSpeed = 7f;
    [SerializeField] GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    private Vector3 lastInteractionDir;
    private ClearCounter selectedCounter;

    private bool isWalking;

    private void Start(){
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e){
        if(selectedCounter != null){
            selectedCounter.Interact();
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
            if(raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)){
                // has clear counter
                if(clearCounter != selectedCounter){
                    SetSelectedCounter(clearCounter);
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
            canMove = !Physics.CapsuleCast(transform.position,transform.position + Vector3.up*playerHeight,  playerRadius, moveDirX, moveDistance);

            if(canMove){
                //Can move only on the X
                moveDir = moveDirX;
            }
            else{
                //Can't move only on the x

                //Attempt on the z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = !Physics.CapsuleCast(transform.position,transform.position + Vector3.up*playerHeight,  playerRadius, moveDirZ, moveDistance);

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

    private void SetSelectedCounter(ClearCounter selectedCounter){
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs{
            selectedCounter = selectedCounter
        });
    }
}
