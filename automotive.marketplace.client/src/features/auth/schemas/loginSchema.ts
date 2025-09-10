import { VALIDATION } from "@/constants/validation";
import { validation } from "@/utils/validation";
import { z } from "zod";

export const LoginSchema = z.object({
  email: z.email().max(VALIDATION.NAME.LONG, {
    error: validation.max({ label: "Email", length: VALIDATION.NAME.LONG }),
  }),
  password: z
    .string()
    .min(VALIDATION.PASSWORD.MIN, {
      error: validation.min({
        label: "Password",
        length: VALIDATION.PASSWORD.MIN,
      }),
    })
    .max(VALIDATION.PASSWORD.MAX, {
      error: validation.max({
        label: "Password",
        length: VALIDATION.PASSWORD.MAX,
      }),
    }),
});
