import { VALIDATION } from "@/constants/validation";
import { validation } from "@/utils/validation";
import { z } from "zod";

export const RegisterSchema = z.object({
  username: z
    .string()
    .min(VALIDATION.USERNAME.MIN, {
      error: validation.min({
        label: "Username",
        length: VALIDATION.USERNAME.MIN,
      }),
    })
    .max(VALIDATION.USERNAME.MAX, {
      error: validation.max({
        label: "Username",
        length: VALIDATION.USERNAME.MAX,
      }),
    }),
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
