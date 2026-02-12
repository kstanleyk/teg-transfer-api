//using TegWallet.Domain.Entity.Core;

//COMMANDS
//1. Client Initiation & Verification
//StartKycProcessCommand - Initialize KYC for a new/ existing client

//SendEmailVerificationCodeCommand - Send verification code to email

//VerifyEmailCommand - Verify email with confirmation code

//SendPhoneVerificationCodeCommand - Send SMS verification code

//VerifyPhoneCommand - Verify phone with SMS code

//ResendVerificationCodeCommand - Resend verification code

//2. Document Management
//UploadDocumentCommand - Upload KYC document with metadata

//SubmitDocumentForVerificationCommand - Submit uploaded document for review

//UpdateDocumentInfoCommand - Update document metadata

//DeleteDocumentCommand - Remove a document

//AddDocumentFrontImageCommand - Attach front image to document

//AddDocumentBackImageCommand - Attach back image to document

//AddSelfieImageCommand - Upload selfie for facial verification

//3. Verification & Review
//SubmitKycForReviewCommand - Submit completed KYC for admin review

//ApproveDocumentCommand - Admin approves a specific document

//RejectDocumentCommand - Admin rejects a document with reason

//RequestAdditionalDocumentsCommand - Request more documents from client

//MarkDocumentUnderReviewCommand - Mark document as under review

//4. KYC Level Management
//ApproveKycLevel1Command - Approve Level 1 (basic info) verification

//ApproveKycLevel2Command - Approve Level 2 (identity) verification

//ApproveKycLevel3Command - Approve Level 3 (enhanced) verification

//AdvanceToLevel3Command - Request Level 3 verification

//CompleteLevel3VerificationCommand - Complete Level 3 verification

//5. KYC Status Management
//ApproveKycCommand - Final KYC approval with expiry date

//RejectKycCommand - Reject KYC with reason

//SuspendKycCommand - Suspend verified KYC

//ReinstateKycCommand - Reinstate suspended KYC

//UpdateKycExpiryCommand - Extend KYC expiry date

//MarkKycExpiredCommand - Manually mark KYC as expired

//6. Third-Party Integration
//VerifyDocumentWithThirdPartyCommand - Send document to third-party provider

//PerformFacialVerificationCommand - Run facial recognition check

//PerformBackgroundCheckCommand - Run background/sanctions check

//VerifyAddressWithThirdPartyCommand - Verify address with external provider

//UpdateThirdPartyVerificationStatusCommand - Update status from third-party webhook

//7. Transaction Compliance
//CheckDepositEligibilityCommand - Check if client can make deposit

//CheckPurchaseEligibilityCommand - Check if client can make purchase

//CheckWithdrawalEligibilityCommand - Check if client can make withdrawal

//CheckInternationalTransferEligibilityCommand - Check international transfer eligibility

//8. Admin Operations
//BulkApproveDocumentsCommand - Approve multiple documents at once

//UpdateClientRiskLevelCommand - Update client risk assessment

//OverrideKycDecisionCommand - Admin override for special cases

//ForceKycVerificationCommand - Force verify KYC (emergency/exception)

//QUERIES
//1. Client KYC Status
//GetKycStatusQuery - Get current KYC status for a client

//GetKycProfileQuery - Get full KYC profile details

//GetKycVerificationHistoryQuery - Get KYC status change history

//GetKycLevelDetailsQuery - Get details about specific KYC level

//GetKycExpiryInfoQuery - Get KYC expiry information

//2. Document Queries
//GetClientDocumentsQuery - Get all documents for a client

//GetDocumentDetailsQuery - Get specific document details

//GetPendingDocumentsQuery - Get pending verification documents

//GetVerifiedDocumentsQuery - Get verified documents

//GetExpiringDocumentsQuery - Get documents expiring soon

//GetDocumentVerificationHistoryQuery - Get document verification attempts

//3. Verification Status
//GetEmailVerificationStatusQuery - Get email verification status

//GetPhoneVerificationStatusQuery - Get phone verification status

//GetIdentityVerificationStatusQuery - Get identity verification status

//GetThirdPartyVerificationStatusQuery - Get third-party verification results

//4. Admin & Reporting
//GetPendingKycReviewsQuery - Get KYC profiles pending review

//GetKycStatisticsQuery - Get KYC processing statistics

//GetKycComplianceReportQuery - Generate compliance report

//GetExpiringKycProfilesQuery - Get KYC profiles expiring soon

//GetSuspendedKycProfilesQuery - Get suspended KYC profiles

//GetRejectedKycProfilesQuery - Get rejected KYC profiles

//GetKycLevelDistributionQuery - Get distribution across KYC levels

//5. Transaction Eligibility
//CheckTransactionEligibilityQuery - Check eligibility for specific transaction

//GetClientTransactionLimitsQuery - Get transaction limits based on KYC level

//VerifyPurchaseEligibilityQuery - Verify purchase eligibility with amount

//GetInternationalTransferEligibilityQuery - Check international transfer eligibility

//6. Compliance & Risk
//GetClientRiskAssessmentQuery - Get client risk assessment

//GetSanctionsCheckResultQuery - Get sanctions check results

//GetBackgroundCheckResultQuery - Get background check results

//GetPepStatusQuery - Get Politically Exposed Person status

//GetComplianceScoreQuery - Get overall compliance score

//7. Audit & Monitoring
//GetKycAuditTrailQuery - Get complete KYC audit trail

//GetDocumentUploadHistoryQuery - Get document upload history

//GetVerificationAttemptsQuery - Get verification attempts history

//GetAdminActionsQuery - Get admin actions on KYC profiles

//8. Search & Filter
//SearchKycProfilesQuery - Search KYC profiles by criteria

//FilterKycByStatusQuery - Filter KYC profiles by status

//FilterKycByLevelQuery - Filter KYC profiles by level

//FilterKycByDateRangeQuery - Filter KYC profiles by date range

//FindDuplicateDocumentsQuery - Find potential duplicate documents