<script lang="ts">
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {actionToString, type RuleGroup, triggerToString} from "$lib/rules";
    import SimpleExpandingList from "$lib/components/SimpleExpandingList.svelte";
    import {Dialog} from "$lib/dialog.svelte";
    import DefaultDialog from "$lib/components/dialog/DefaultDialog.svelte";
    import LabeledInput from "$lib/components/LabeledInput.svelte";

    let ruleGroups = $state(load())

    async function load() {
        let res = await apiClient.get<RuleGroup[]>("rule-groups");
        return res.data;
    }

    let ruleDialog = new Dialog();

    let ruleGroupDialog = new Dialog();
    let ruleGroupUpdate = $state<{ previous?: string; current: string; }>({
        previous: undefined,
        current: ""
    })
    const ruleGroupDialogHeader = $derived(ruleGroupUpdate.previous ? "Update" : "New");

    function openRuleGroupDialog(previousValue?: string) {
        ruleGroupUpdate.previous = previousValue;
        ruleGroupUpdate.current = previousValue ?? "";
        ruleGroupDialog.open();
    }

    async function saveRuleGroup() {
        const payload = {name: ruleGroupUpdate.current};
        if (ruleGroupUpdate.previous) {
            const res = await apiClient.put(`rule-groups/${ruleGroupUpdate.previous}`, payload);
        } else {
            const res = await apiClient.post(`rule-groups`, payload);
        }

        ruleGroups = load();
    }

    let deleteConfirmationDialog = new Dialog();
</script>

<div class="mb-8 flex gap-4 items-center">
    <h1 class="h1">Rules</h1>
    <button onclick={() => openRuleGroupDialog()} class="btn variant-filled-secondary">New Group</button>
</div>

{#await ruleGroups}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then groups}
    {#each groups as group}
        <div class="card mb-8 p-4">
            <div class="flex justify-between mb-4">
                <div class="flex gap-4">
                    <h2 class="h2">{group.name || "(ungrouped)"}</h2>
                    {#if group.name}
                        <button
                            class="btn variant-outline-tertiary"
                            onclick={() => openRuleGroupDialog(group.name)}
                        >Edit
                        </button>
                    {/if}
                </div>
                <button class="btn variant-outline-secondary">New Rule</button>
            </div>
            <div class="table-container">
                <table class="table table-fixed">
                    <thead>
                    <tr>
                        <th> Name</th>
                        <th>Triggers when</th>
                        <th>Rule will</th>
                    </tr>
                    </thead>
                    <tbody>
                    {#each group.rules as rule}
                        <tr>
                            <td>
                                <a class="text-primary-400" href="rules/{rule.id}">
                                    {rule.name}
                                </a>
                            </td>
                            <td>
                                <SimpleExpandingList
                                    items={rule.triggers}
                                    stringify={triggerToString}
                                    label="Triggers"
                                />
                            </td>
                            <td>
                                <SimpleExpandingList
                                    items={rule.actions}
                                    stringify={actionToString}
                                    label="Actions"
                                />
                            </td>
                        </tr>
                    {/each}
                    </tbody>
                </table>
            </div>
        </div>
    {/each}
    <DefaultDialog>
        <h2 class="h2">Rules</h2>
    </DefaultDialog>
    <DefaultDialog bind:dialog={ruleGroupDialog.value}>
        <div class="flex flex-col gap-8 items-center">
            <h2 class="h2">{ruleGroupDialogHeader} Rule Group</h2>
            <LabeledInput
                type="text"
                bind:value={ruleGroupUpdate.current}
                label="Name"
            />
            <div class="flex flex-row gap-8">
                <button onclick={() => ruleGroupDialog.close()} class="btn variant-outline-error">Cancel</button>
                <button onclick={saveRuleGroup} class="btn variant-filled-success">Save</button>
            </div>
        </div>
    </DefaultDialog>
    <DefaultDialog>
        <h2 class="h2">Are you sure you want to delete?</h2>
    </DefaultDialog>
{/await}
