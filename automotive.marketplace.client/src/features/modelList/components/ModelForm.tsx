import DatePicker from "@/components/common/DatePicker";
import MakeSelect from "@/components/forms/MakeSelect";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { modelFormSchema } from "../schemas/modelFormSchema";
import { ModelFormData } from "../types/ModelFormData";

type ModelFormProps = {
  model: ModelFormData;
  onSubmit: (formData: ModelFormData) => Promise<void>;
  className?: string;
};

const ModelForm = ({ model, onSubmit, className }: ModelFormProps) => {
  const form = useForm({
    defaultValues: {
      name: model.name,
      firstAppearanceDate: model.firstAppearanceDate,
      isDiscontinued: model.isDiscontinued,
      makeId: model.makeId,
    },
    resolver: zodResolver(modelFormSchema),
  });

  console.log("model: ", model);

  return (
    <div className={cn(className)}>
      <Form {...form}>
        <form
          className="grid w-full min-w-3xs gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
          onSubmit={form.handleSubmit(onSubmit)}
        >
          <FormField
            name="name"
            control={form.control}
            render={({ field }) => (
              <FormItem>
                <FormLabel>Model name</FormLabel>
                <FormControl>
                  <Input type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="firstAppearanceDate"
            control={form.control}
            render={({ field }) => (
              <FormItem>
                <FormLabel>First appearance date</FormLabel>
                <FormControl>
                  <DatePicker field={field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isDiscontinued"
            control={form.control}
            render={({ field }) => (
              <FormItem>
                <FormLabel>Is discontinued</FormLabel>
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem>
                <FormLabel>Make</FormLabel>
                <FormControl>
                  <MakeSelect isAllMakesEnabled={false} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit">Confirm</Button>
        </form>
      </Form>
    </div>
  );
};

export default ModelForm;
