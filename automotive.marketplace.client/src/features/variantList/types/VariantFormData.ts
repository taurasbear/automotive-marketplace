import z from "zod";
import { variantFormSchema } from "../schemas/variantFormSchema";

export type VariantFormData = z.infer<typeof variantFormSchema>;
