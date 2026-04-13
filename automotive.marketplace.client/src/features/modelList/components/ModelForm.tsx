import MakeSelect from "@/components/forms/select/MakeSelect";
import { Button } from "@/components/ui/button";
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
      makeId: model.makeId,
    },
    resolver: zodResolver(modelFormSchema),
  });

  const handleSubmit = async (formData: ModelFormData) => {
    await onSubmit(formData);
    form.reset();
  };

  return (
    <div className={cn(className)}>
      <Form {...form}>
        <form
          className="grid w-full min-w-3xs gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
          onSubmit={form.handleSubmit(handleSubmit)}
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
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem>
                <FormLabel>Make</FormLabel>
                <FormControl>
                  <MakeSelect
                    isAllMakesEnabled={false}
                    onValueChange={field.onChange}
                    {...field}
                  />
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
