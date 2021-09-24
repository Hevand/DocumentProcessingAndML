import json
import os
from transformers import pipeline

# Configuration data. They might be loaded from separate files - but let's hardcode for now.
questions = [
    'What is the property address?',
    'Who is the landlord?',
    'Who is the tenant?',
    'What is the Lease Signed Date?',
    'What is the Rent Start Date?',
    'What is the Expiration Date?',
    'What is the amount of the (security) deposit?',
    'Lease/Rent Renewal Terms?',
    'Improvement Allowance?',
    'Landlord/Tenant Reimbursements?',
    'Landlord/Tenant Maintenance?',
    'What is the possession date?',
    'Is this an operating lease?',
    'Is this a capital lease?',
    'What is the Base Rent/Billing Frequency?',
    'What are the Rent Escalations, if any?',
    'Base Year or Expense Stop?',
    'Expense Escalation?',
    'Who pays the utilities?',
    'Rentable Square Feet?',
    'What is the Free Rent Period, if any?',
    'For retail spaces, percentage of gross receipts charged as rent?',
    'Other Rent (storage/parking/contingency)?',
    'Considerations and additional comments for the Engagement Team?'
]

fact_names = [
    'Property Address',
    'Landlord',
    'Tenant',
    'Lease Signed Date',
    'Rent Start Date',
    'Expiration Date',
    'Security Deposit Provisions',
    'Lease/Rent Renewal Terms',
    'Improvement Allowance',
    'Landlord/Tenant Reimbursements ',
    'Landlord/Tenant Maintenance ',
    'Date tenant had ability to occupy the rental space',
    'Operating Lease',
    'Capital Lease',
    'Base Rent/Billing Frequency',
    'Rent Escalations',
    'Base Year or Expense Stop',
    'Expense Escalation',
    'Utility Reimbursement',
    'Rentable Square Feet',
    'Free Rent Period and Amount',
    'For retail spaces, percentage of gross receipts charged as rent',
    'Other Rent (storage/parking/contingency)',
    'Considerations and additional comments for the Engagement Team'    
]

def init():
    model = 'bert-large-uncased-whole-word-masking-finetuned-squad'
    global question_answerer 
    
    # device should be 0 for GPU or -1 for CPU. GPU is much (much) faster
    question_answerer = pipeline("question-answering", model=model, device=-1)

def run(request):    
    text = json.loads(request)
    identifier = text["id"]
    documentText = text["content"]

    #Run inference
    answer_top3 = question_answerer(
        question = questions,
        context = documentText,
        topk=3
    )
    
    result = buildresult(identifier, answer_top3)
    return json.dumps(result)

def buildresult(identifier, answer_top3):
    result={ "id" : identifier }
    result["questions"] = []
    i = 0
    for question in questions:
        item = {"id" : fact_names[i]}        
        item["answers"] = []        
        for answerIndex in [0,1,2]:
            answer = answer_top3[answerIndex] # should be: i * 3 + answerIndex
            item["answers"].append(answer)    
        
        result["questions"].append(item)
        i += 1

    return result