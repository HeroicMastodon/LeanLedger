<div class="mb-8 flex gap-4 items-center">
    <h1 class="h1">Accounts</h1>
    <FormButton
        class="btn-icon-sm p-2 variant-outline-primary text-primary-500"
        text="New Account"
        onConfirm={saveNewAccount}
        confirmText="Create"
        icon={faPlus}
    >
        <AccountForm bind:account={newAccount} />
    </FormButton>
</div>
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <Accordion>
        {@render accountTable("Bank", true, accounts)}
        {@render accountTable("CreditCard", true, accounts)}
        {@render accountTable("Merchant", false, accounts)}
    </Accordion>
{/await}

{#snippet accountTable(accountType: AccountType, open: boolean, accounts: AccountGrouping)}
    <AccordionItem open={open}>
        {#snippet summary()}
            {splitPascal(accountType)}s
        {/snippet}
        {#snippet content()}
            <div class="table-container">
                <table class="table table-compact table-hover md:table-fixed">
                    <thead>
                    <tr>
                        <th>Name</th>
                        <th>Balance</th>
                        <th>Change</th>
                        <th class="hidden md:table-cell">Active</th>
                        <th class="hidden md:table-cell">Last Activity</th>
                    </tr>
                    </thead>
                    <tbody>
                    {#each accounts[accountType] as account}
                        <tr class="hover:cursor-pointer" onclick={() => goto(`/accounts/${account.id}?${monthManager.params}`)}>
                            <td>
                                {account.name}
                            </td>
                            <td>
                                <Money amount={account.balance} />
                            </td>
                            <td>
                                <Money amount={account.balanceChange} />
                            </td>
                            <td class="hidden md:table-cell">
                                <input type="checkbox" class="checkbox" checked={account.active} disabled />
                            </td>
                            <td class="hidden md:table-cell">{account.lastActivityDate ?? "No Activity"}</td>
                        </tr>
                    {/each}
                    </tbody>
                </table>
            </div>
        {/snippet}
    </AccordionItem>

{/snippet}

<script lang="ts">
    import {apiClient} from "$lib/apiClient";
    import {Accordion, AccordionItem, ProgressBar} from "@skeletonlabs/skeleton";
    import {splitPascal} from "$lib";
    import {type AccountData, type AccountGrouping, type AccountType, defaultAccountData,} from "$lib/accounts";
    import {goto} from "$app/navigation";
    import Money from "$lib/components/Money.svelte";
    import AccountForm from "$lib/accounts/AccountForm.svelte";
    import DefaultDialog from "$lib/components/dialog/DefaultDialog.svelte";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import {monthManager} from "$lib/selectedMonth.svelte";
    import {faPlus} from "@fortawesome/free-solid-svg-icons/faPlus";

    let accounts: AccountGrouping = $state({
        Bank: [],
        CreditCard: [],
        Merchant: []
    });

    async function load() {
        let response = await apiClient.get<AccountGrouping>(`accounts?${monthManager.params}`);
        accounts = response.data;
    }

    let newAccount: AccountData = $state(defaultAccountData());

    async function saveNewAccount() {
        const response = await apiClient.post<AccountData>("accounts", newAccount);
        await load();
        newAccount = defaultAccountData();
        return true;
    }
</script>
