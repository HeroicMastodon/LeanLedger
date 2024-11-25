<script lang="ts">
	import '../app.postcss';

	// Floating UI for Popups
	import { computePosition, autoUpdate, flip, shift, offset, arrow } from '@floating-ui/dom';
	import { storePopup } from '@skeletonlabs/skeleton';
	interface Props {
		children?: import('svelte').Snippet;
	}

	let { children }: Props = $props();
	storePopup.set({ computePosition, autoUpdate, flip, shift, offset, arrow });
</script>

{@render children?.()}
