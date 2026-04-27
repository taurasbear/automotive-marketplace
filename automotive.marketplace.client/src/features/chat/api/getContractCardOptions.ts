import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { ContractCard } from "../types/ContractCard";

const getContractCard = (contractCardId: string) =>
  axiosClient.get<ContractCard>(ENDPOINTS.CHAT.GET_CONTRACT_CARD, {
    params: { contractCardId },
  });

export const getContractCardOptions = (contractCardId: string) =>
  queryOptions({
    queryKey: chatKeys.contractCard(contractCardId),
    queryFn: () => getContractCard(contractCardId),
    enabled: !!contractCardId,
  });
