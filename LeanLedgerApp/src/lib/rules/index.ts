import {splitPascal} from "$lib";

export type RuleTrigger = {
    field: TransactionRuleField;
    not: boolean;
    condition: RuleCondition;
    value?: string;
}
export type RuleAction = {
    actionType: RuleActionType;
    field?: TransactionRuleField;
    value?: string;
}
export const TransactionRuleFields = [
    "Description",
    "Date",
    "Amount",
    "Type",
    "Category",
    "Source",
    "Destination"
] as const;
export type TransactionRuleField = typeof TransactionRuleFields[number];
export const RuleConditions = [
    "StartsWith",
    "EndsWith",
    "Contains",
    "IsExactly",
    "GreaterThan",
    "LessThan",
    "Exists",
] as const;
export type RuleCondition = typeof RuleConditions[number];
export const RuleActionTypes = [
    "Append",
    "Set",
    "Clear",
    "DeleteTransaction"
] as const;
export type RuleActionType = typeof RuleActionTypes[number];
export type RuleGroup = {
    name?: string;
    rules: Rule[];
}
export type Rule = {
    id: string;
    name: string;
    isStrict: boolean;
    triggers: RuleTrigger[];
    actions: RuleAction[];
}

export function triggerToString(trigger: RuleTrigger) {
    return `When ${splitPascal(trigger.field)} ${trigger.not ? "Not " : ""}${splitPascal(trigger.condition)} ${trigger.value || ""}`
}
export function actionToString(action: RuleAction) {
    let result = splitPascal(action.actionType);

    if (action.field) {
        result += ` ${action.field} to ${action.value}`
    }

    return result;
}
