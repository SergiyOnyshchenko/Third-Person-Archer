using Actor;
using UnityEngine;
using UnityEngine.AI;

public class TestFlow : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private MainState _state;

    private void Awake()
    {
        Player player = FindAnyObjectByType<Player>();

        player.GetComponent<NavMeshAgent>().enabled = false;
        player.transform.position = _spawnPoint.position;
        player.GetComponent<NavMeshAgent>().enabled = true;
        GetComponentInChildren<OnClickTransition>().SetNextStateManualy(_state);
    }
}
