using UnityEngine;
using UnityEngine.UI;

// Script populates the hot bar in build mode, should be intergrated into the UI controller
public class ToolBarBuildButtons : MonoBehaviour
{
    // Button prefab.
    public GameObject BuildButtonPrefab;
    // Array containing all the ship components.
    public GameObject[] ShipPartPrefabsArray;
    ModelManger ModelManger;
    DecalManager DecalManager;

    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to the model manager.
        ModelManger = GameObject.FindObjectOfType<ModelManger>();
        DecalManager = GameObject.FindObjectOfType<DecalManager>();

        // For every gameobject in the ShipPartPrefabs Array:
        foreach (GameObject shipPart in ShipPartPrefabsArray)
        {
            // Spawn a button, as a child of the tool bar, and change the text in the button to the prefab name.
            GameObject buttonGameObject = (GameObject)Instantiate(BuildButtonPrefab, this.transform);
            Text buttonLabel = buttonGameObject.GetComponentInChildren<Text>();
            buttonLabel.text = shipPart.name;

            // Modify the button component, to add the on click event, 
            // that changes the object to place in the model manager to the ship part the button represents.
            Button theButton = buttonGameObject.GetComponent<Button>();
            if (transform.parent.transform.parent.gameObject.TryGetComponent(typeof(EditButtonHandler), out Component EditButtonHandlerUI))
            {
                theButton.onClick.AddListener(() => { ModelManger.ObjectToPlace = shipPart; });
            }
            else if(transform.parent.transform.parent.gameObject.TryGetComponent(typeof(DecalManagerUI),out Component DecalManagerUI))
            {
                theButton.onClick.AddListener(() => { DecalManager.DecalToPlace = shipPart; });
            }
        }
    }
}
