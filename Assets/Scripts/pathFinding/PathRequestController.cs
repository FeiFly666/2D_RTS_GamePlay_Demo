using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequest
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public System.Action<List<Node>, bool> callback;
    public object requester;

    public PathRequest(Vector3 start, Vector3 end, System.Action<List<Node>, bool> cb, object req)
    {
        startPosition = start;
        endPosition = end;
        callback = cb;
        requester = req;
    }
}
public class PathRequestController : MonoSingleton<PathRequestController>
{
    //private List<PathRequest> requestList = new List<PathRequest>();

    private Queue<PathRequest> requestList = new Queue<PathRequest>();

    private Dictionary<object, PathRequest> requestMap = new Dictionary<object, PathRequest>();

    private PathRequest currentRequest;
    private bool isProcessing;
    [SerializeField] private int maxProcessedPerFrame = 8;

    private void Update()
    {
        TryProcessBatch();
    }

    private void TryProcessBatch()
    {
        int countThisFrame = 0;

        if (Time.deltaTime > 0.018f)
            maxProcessedPerFrame = 4;
        else
            maxProcessedPerFrame = 8;

        while (!isProcessing && requestList.Count > 0 && countThisFrame < maxProcessedPerFrame)
        {
            countThisFrame++;
            TryProcessNext();
        }
    }


    public static void RequestPath(Vector3 start, Vector3 end, System.Action<List<Node>, bool> callback, object requester)
    {
        Instance.AddRequest(start, end, callback, requester);
    }

    private void AddRequest(Vector3 start, Vector3 end, System.Action<List<Node>, bool> callback, object requester)
    {
        /*if (requester != null)
        {
            // 在队列中寻找是否已经有该请求者发出的请求
            PathRequest existingRequest = requestList.Find(r => r.requester == requester);

            if (existingRequest != null)
            {
                existingRequest.startPosition = start;
                existingRequest.endPosition = end;
                existingRequest.callback = callback;
                return;
            }
        }

        requestList.Add(new PathRequest(start, end, callback, requester));*/


        if(currentRequest != null && currentRequest.requester == requester)
        {
            currentRequest.startPosition = start;
            currentRequest.endPosition = end;
            currentRequest.callback = callback;
            return;
        }

        //去重
        if(requester != null && requestMap.TryGetValue(requester, out var existingRequest))
        {
            existingRequest.startPosition = start;
            existingRequest.endPosition = end;
            existingRequest.callback = callback;
            return;
        }

        var request = new PathRequest(start, end, callback, requester);

        requestList.Enqueue(request);
        if(requester != null)
        {
            requestMap[requester] = request;
        }
        //TryProcessNext();
    }

    private void TryProcessNext()
    {
        if (!isProcessing && requestList.Count > 0)
        {
            currentRequest = requestList.Dequeue();

            if(currentRequest.requester != null)
            {
                requestMap.Remove(currentRequest.requester);

                if (currentRequest.requester is MonoBehaviour mono && mono == null)
                {
                    return;
                }

            }

            isProcessing = true;

            TilemapManager.Instance.FindPathFrameByFrame(currentRequest.startPosition, currentRequest.endPosition);
        }
    }

    public void FinishedProcessing(List<Node> path, bool success)
    {
        currentRequest?.callback?.Invoke(path, success);
        isProcessing = false;
        currentRequest = null;
        //TryProcessNext();
    }
}