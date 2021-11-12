using UnityEngine;

public class SelectionComponent : MonoBehaviour
{
    public Material Orignal;
    public Material Selection;
    public bool SetMat;
    // Start is called before the first frame update
    void Start()
    {
        Orignal = GetComponent<MeshRenderer>().material;
        Vector3 heading = Camera.main.transform.position - this.transform.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        Ray ray = new Ray(this.transform.position, direction);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, distance + 10.0f))
        {
            if (!hitInfo.transform.gameObject.CompareTag("MainCamera"))
            {
                Destroy(this);
            }
        }
        
        if(SetMat)
            GetComponent<MeshRenderer>().material = Selection;
    }

    private void OnDestroy()
    {
        Debug.Log(this.gameObject.name + " SelectionComponent Destroyed");
        if (SetMat)
            GetComponent<MeshRenderer>().material = Orignal;
    }
}
