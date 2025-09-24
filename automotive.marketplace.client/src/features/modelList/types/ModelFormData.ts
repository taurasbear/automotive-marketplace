import z from "zod";
import { modelFormSchema } from "../schemas/modelFormSchema";

export type ModelFormData = z.infer<typeof modelFormSchema>;
