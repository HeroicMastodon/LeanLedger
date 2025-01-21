<script lang="ts">
    import '../app.postcss';

    // Floating UI for Popups
    import {computePosition, autoUpdate, flip, shift, offset, arrow} from '@floating-ui/dom';
    import {AppBar, AppShell, storePopup} from '@skeletonlabs/skeleton';
    import {Fa} from "svelte-fa";
    import {faCaretDown, faDollar, faPalette,} from "@fortawesome/free-solid-svg-icons";
    import {popup} from "@skeletonlabs/skeleton";

    import {page} from "$app/stores";

    import {monthManager} from "$lib/selectedMonth.svelte";
    import {afterNavigate, beforeNavigate} from "$app/navigation";
    import {faArrowLeft} from "@fortawesome/free-solid-svg-icons/faArrowLeft";
    import {faArrowRight} from "@fortawesome/free-solid-svg-icons/faArrowRight";

    interface Props {
        children?: import('svelte').Snippet;

        [key: string]: any
    }

    let props: Props = $props();
    storePopup.set({computePosition, autoUpdate, flip, shift, offset, arrow});

    const themes = ['carbon-fox', 'wintry'] as const;
    let selectedTheme = $state('carbon-fox');

    afterNavigate(() => {
        monthManager.selectFromQuery($page.url.searchParams)
    })

    function isActive(path: string, href: string) {
        const withoutLeadingSlash = path.substring(1);
        const withoutQuery = href.split("?")[0];
        const firstInPath = withoutQuery.split("/")[1];

        return withoutLeadingSlash.startsWith(firstInPath)
    }

    const isControlledByGlobalMonth = $derived.by(() => {
        const path = $page.url.pathname.toLowerCase();

        return !(path.startsWith("/transaction") || path.startsWith("/rules"));
    })
    const selectedMonthParams = $derived(`?year=${monthManager.selectedMonth.year}&month=${monthManager.selectedMonth.number}`)
</script>
{#snippet navItem(href: string, title: string)}
    <li>
        <a href="{href}"
           class:bg-primary-active-token={isActive($page.url.pathname, href)}
           data-sveltekit-preload-data="hover"
        >
            <span class="flex-auto">{title}</span>
        </a>
    </li>
{/snippet}

<AppShell>
    {#snippet header()}

        <!-- App Bar -->
        <AppBar>
            {#snippet lead()}
                <a aria-label="Home" class="btn-icon btn-icon-xl" href="/{selectedMonthParams}">
                    <Fa icon={faDollar} />
                </a>
            {/snippet}
            <a href="/{selectedMonthParams}" class="h2 font-bold">
                Lean Ledger
            </a>
            {#snippet trail()}
                {#if isControlledByGlobalMonth}
                    <div class="flex items-center">
                        <p class="p">{monthManager.selectedMonth.name} {monthManager.selectedMonth.year}</p>
                        <a href="{$page.url.pathname}?month={monthManager.lastMonth.number}&year={monthManager.lastMonth.year}"
                           class="btn btn-icon text-tertiary-500"
                        >
                            <Fa icon={faArrowLeft} />
                        </a>
                        <a href="{$page.url.pathname}?year={monthManager.nextMonth.year}&month={monthManager.nextMonth.number}"
                           class="btn btn-icon text-tertiary-500 ml-[-1rem]"
                        >
                            <Fa icon={faArrowRight} />
                        </a>
                    </div>
                {/if}
                    <button class="btn hover:variant-soft-primary"
                            use:popup={{ event: 'click', target: 'theme', closeQuery: 'a[href]' }}
                    >
                        <Fa icon={faPalette} class="svelte-fa-size-lg md:!hidden"></Fa>
                        <span class="hidden md:inline-block">Theme</span>
                        <Fa icon={faCaretDown} class="fa-solid opacity-50"></Fa>
                    </button>
                    <div class="card p-4 w-60 shadow-xl" data-popup="theme">
                        <div class="flex flex-col gap-4">
                            {#each themes as theme}
                                <button
                                    class="btn"
                                    class:bg-primary-active-token={selectedTheme === theme}
                                    onclick={() => {
                                selectedTheme = theme;
                                document.body.setAttribute('data-theme', theme);
                            }}
                                >{theme}</button>
                            {/each}
                        </div>
                    </div>
                    {/snippet}
                    </AppBar>

                    {/snippet}
                    {#snippet sidebarLeft()}
                        <!-- Nav List -->
                        <div class="h-full bg-surface-50-900-token border-r border-surface-500/30 {props.class ?? ''}">
                            <section class="p-4 pb-20 space-y-4 overflow-y-auto">
                                <nav class="list-nav">
                                    <ul>
                                        {@render navItem(`/accounts${selectedMonthParams}`, "Accounts")}
                                        {@render navItem("/transactions", "Transactions")}
                                        {@render navItem(`/categories${selectedMonthParams}`, "Categories")}
                                        {@render navItem("/rules", "Rules")}
                                        {@render navItem(`/budgets${selectedMonthParams}`, "Budgets")}
                                    </ul>
                                </nav>
                            </section>
                        </div>

                    {/snippet}
                    <!-- Page Route Content -->
                    <section class="m-8">
                        {@render props.children?.()}
                    </section>

                    </AppShell>
