import { join } from 'path'
import type { Config } from 'tailwindcss'
import forms from '@tailwindcss/forms';
import { skeleton } from '@skeletonlabs/tw-plugin'
import {carbonFox} from './carbon-fox';

export default {
	darkMode: 'class',
	content: ['./src/**/*.{html,js,svelte,ts}', join(require.resolve('@skeletonlabs/skeleton'), '../**/*.{html,js,svelte,ts}')],
	theme: {
		extend: {},
	},
	plugins: [
		forms,
		skeleton({
			themes: {
                custom: [
                    carbonFox,
                ],
				preset: [
					{
						name: 'wintry',
						enhancements: true,
					},
				],
			},
		}),
	],
    safelist: [
        "variant-outline-error",
        "variant-outline-primary",
        "variant-outline-warning",
        "variant-outline-success",
        "variant-filled-error",
        "variant-filled-primary",
        "variant-filled-warning",
        "variant-filled-success",
    ],
} satisfies Config;
