using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerGridInputHandler : Assets._Scripts.Utilities.Singleton.Singleton<PlayerGridInputHandler> {

    [SerializeField] private InputActionReference tap;
    private InputAction tapAction;
    private Grid grid;

    //public static event System.Action<Vector2Int> OnTileClicked;

    private void Start() {
        this.grid = GridController.Instance.Grid;
    }

    private void OnEnable() {
        this.tapAction = tap.action;
        this.tapAction.Enable();
        this.tapAction.performed += OnPointerClick;
    }

    private void OnDisable() {
        this.tapAction.performed -= OnPointerClick;
        this.tapAction.Disable();
    }

    public void OnPointerClick(InputAction.CallbackContext context) {
        if (context.performed) {
            // Check if pointer is over a UI element. If so, ignore the world click.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
                return;
            }

            // Read pointer position from the New Input System (works for Touch and Mouse)
            Vector2 screenPosition = Pointer.current.position.ReadValue();

            // Convert to World, then to Grid 
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;

            // Get the integer index of the cell
            Vector3Int cellPosition = this.grid.WorldToCell(worldPosition);

            // Pass this to the GridController
            GridController.Instance.InteractWithTile(cellPosition);
        }
    }
}