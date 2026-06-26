# Workflow Service

## Trách nhiệm

- Định nghĩa quy trình phê duyệt.
- Chạy workflow instance.
- Ghi action duyệt/từ chối/ủy quyền.

## Data dự kiến

- WorkflowDefinitions
- WorkflowSteps
- WorkflowInstances
- WorkflowActions

## API dự kiến

- `POST /workflow-definitions`
- `POST /workflow-instances`
- `POST /workflow-instances/{id}/approve`
- `POST /workflow-instances/{id}/reject`

## Events

- `WorkflowStarted`
- `WorkflowApproved`
- `WorkflowRejected`
