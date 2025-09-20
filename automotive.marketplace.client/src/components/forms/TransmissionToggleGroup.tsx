import { getTransmissionTypesOptions } from "@/api/enum/getTransmissionTypesOptions";
import { toggleVariants } from "@/components/ui/toggle";
import { cn } from "@/lib/utils";
import * as ToggleGroupPrimitive from "@radix-ui/react-toggle-group";
import { useQuery } from "@tanstack/react-query";
import { type VariantProps } from "class-variance-authority";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";

type TransmissionToggleGroupProps = React.ComponentProps<
  typeof ToggleGroupPrimitive.Root
> &
  VariantProps<typeof toggleVariants>;

const TransmissionToggleGroup = ({
  className,
  ...props
}: TransmissionToggleGroupProps) => {
  const { data: transmissionTypesQuery } = useQuery(
    getTransmissionTypesOptions,
  );

  const transmissionTypes = transmissionTypesQuery?.data || [];

  return (
    <div className={cn(className)}>
      <ToggleGroup {...props}>
        {transmissionTypes.map((transmission) => (
          <ToggleGroupItem
            key={transmission.transmissionType}
            value={transmission.transmissionType}
            className="flex-none"
          >
            {transmission.transmissionType}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
    </div>
  );
};

export default TransmissionToggleGroup;
