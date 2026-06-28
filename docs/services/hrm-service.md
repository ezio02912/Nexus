# HRM Service

## Trách nhiệm

- Quản lý hồ sơ nhân viên đầy đủ: thông tin cá nhân, liên hệ, giấy tờ, thuế, bảo hiểm, ngân hàng, phòng ban, chức vụ, quản lý, trạng thái lao động, lương cơ bản.
- Quản lý phòng ban, chức vụ, dải lương, cost center.
- Quản lý hợp đồng lao động, ngày hiệu lực, ký, file đính kèm và lịch sử thay đổi.
- Quản lý tuyển dụng: requisition, candidate, application, interview, offer.
- Khi offer accepted, service tạo Employee draft và onboarding checklist.

## Data

- Employees
- Departments
- Positions
- EmployeeContracts
- EmployeeHistories
- JobRequisitions
- Candidates
- JobApplications
- Interviews
- Offers
- OnboardingChecklists

## API chính

- `GET/POST /api/hrm/employees`
- `GET/POST /api/hrm/departments`
- `GET/POST /api/hrm/positions`
- `GET/POST /api/hrm/contracts`
- `GET/POST /api/hrm/requisitions`
- `GET/POST /api/hrm/candidates`
- `GET/POST /api/hrm/applications`
- `GET/POST /api/hrm/interviews`
- `GET/POST /api/hrm/offers`
- `POST /api/hrm/offers/{id}/accept`
- `POST /api/hrm/employees/{id}/resign`

## Events

- `EmployeeCreated`
- `EmployeeContractSigned`
- `EmployeeResigned`
- `OfferAccepted`
- `OnboardingChecklistCreated`
